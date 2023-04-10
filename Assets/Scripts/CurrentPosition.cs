using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RedRunner.Characters;
using RedRunner.Enemies;
using RedRunner.Collectables;

public class CurrentPosition : MonoBehaviour
{
    private TextMeshProUGUI printInfo;
    private Character player;
    private Vector2 previousVelocity;
    private Camera cam;
    private bool isTextVisible = false;

    void Start()
    {
        printInfo = GameObject.Find("Information Text").GetComponent<TextMeshProUGUI>();
        printInfo.enabled = false;
        player = GameObject.Find("RedRunner").GetComponent<Character>();
        previousVelocity = Vector2.zero;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    void Update()
    {
        float detectionRangeY = cam.orthographicSize;
        float detectionRangeX = detectionRangeY * cam.aspect;
        
        Vector2 currentVelocity = player.GetComponent<Rigidbody2D>().velocity;
        Vector2 acceleration = (currentVelocity - previousVelocity) / Time.deltaTime;
        previousVelocity = currentVelocity;

        printInfo.text = "Current Position: (" + player.transform.position.x.ToString("N2") + ", " + player.transform.position.y.ToString("N2") + ")\n" +
        "Current Velocity: " + currentVelocity + "\n" +
        "Current Acceleration: " + acceleration + "\n";

        Coin[] allCoins = FindObjectsOfType<Coin>();
        List<Coin> detectedCoins = new List<Coin>();

        foreach (Coin c in allCoins)
        {
            if (c.transform.position.x > cam.transform.position.x - detectionRangeX && c.transform.position.x < cam.transform.position.x + detectionRangeX)
            {
                if (c.transform.position.y > cam.transform.position.y - detectionRangeY && c.transform.position.y < cam.transform.position.y + detectionRangeY)
                {
                    detectedCoins.Add(c);
                }
            }
        }

        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        List<Enemy> detectedEnemies = new List<Enemy>();

        foreach (Enemy e in allEnemies)
        {
            Vector3 exts = e.GetComponent<Collider2D>().bounds.extents;
            
            if (e.transform.position.x > cam.transform.position.x - detectionRangeX - exts.x && e.transform.position.x < cam.transform.position.x + detectionRangeX + exts.x)
            {
                if (e.transform.position.y > cam.transform.position.y - detectionRangeY - exts.y && e.transform.position.y < cam.transform.position.y + detectionRangeY + exts.y)
                {
                    detectedEnemies.Add(e);
                }
            }
        }

        for (int i = 0; i < detectedEnemies.Count  ; i++)
        {
            Vector4 hitBox = ComputeHitBox(detectedEnemies[i]);
            printInfo.text += detectedEnemies[i].GetType().Name + " " + ComputePosition(detectedEnemies[i]) + " [(" + hitBox.x.ToString("N2") + ", " + hitBox.y.ToString("N2") + "), (" + hitBox.z.ToString("N2") + ", " + hitBox.w.ToString("N2") + ")]\n";
        }

        for (int i = 0; i < detectedCoins.Count; i++)
        {
            printInfo.text += detectedCoins[i].GetType().Name + " " + ComputePosition(detectedCoins[i]) + "\n";
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            isTextVisible = !isTextVisible;
            printInfo.enabled = isTextVisible;
        }
    }

    private Vector2 ComputePosition(Component component)
    {
        return new Vector2(component.transform.position.x, component.transform.position.y);
    }
    
    private Vector4 ComputeHitBox(Component component)
    {
        Vector2 edges = new Vector2(component.GetComponent<Collider2D>().bounds.extents.x, component.GetComponent<Collider2D>().bounds.extents.y);
        Vector4 hitBox = new Vector4(component.transform.position.x - edges.x, component.transform.position.y - edges.y, component.transform.position.x + edges.x, component.transform.position.y + edges.y);

        return hitBox;
    }
}
