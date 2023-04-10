using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RedRunner.Characters;
using RedRunner.Enemies;
using RedRunner.Utilities;
using RedRunner.TerrainGeneration;
using System.ComponentModel;
using System.Diagnostics;

public class CurrentPosition : MonoBehaviour
{
    private TextMeshProUGUI printInfo;
    private Character player;
    private Vector2 previousVelocity;
    private List<Enemy> detectedEnemies = new List<Enemy>();
    private float detectionRangeX;
    private float detectionRangeY;
    private Camera cam;
    private TerrainGenerator terrainGenerator;

    void Start()
    {
        printInfo = GameObject.Find("Information Text").GetComponent<TextMeshProUGUI>();
        player = GameObject.Find("RedRunner").GetComponent<Character>();
        previousVelocity = Vector2.zero;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        terrainGenerator = GameObject.Find("Terrain Generator").GetComponent<TerrainGenerator>();
    }

    void Update()
    {
        detectionRangeY = cam.orthographicSize * 2f;
        detectionRangeX = detectionRangeY * cam.aspect;
        
        Vector2 currentVelocity = player.GetComponent<Rigidbody2D>().velocity;
        Vector2 acceleration = (currentVelocity - previousVelocity) / Time.deltaTime;
        previousVelocity = currentVelocity;

        printInfo.text = "Current Position: " + player.transform.position + "\n" +
        "Current Velocity: " + currentVelocity + "\n" +
        "Current Acceleration: " + acceleration + "\n" +
        "Current Hitbox: [" + ComputeHitBox(player)[0] + ", " + ComputeHitBox(player)[1] + ", " + ComputeHitBox(player)[2] + ", " + ComputeHitBox(player)[3] + "]\n";

        detectedEnemies.Clear();

        Enemy[] allEnemies = GameObject.FindObjectsOfType<Enemy>();
        foreach (Enemy e in allEnemies)
        {
            float enemyRadiusX = e.GetComponent<Collider2D>().bounds.extents.x;
            float enemyRadiusY = e.GetComponent<Collider2D>().bounds.extents.y;
            
            if (e.transform.position.x > player.transform.position.x - detectionRangeX / 2f - enemyRadiusX && e.transform.position.x < player.transform.position.x + detectionRangeX / 2f + enemyRadiusX)
            {
                if (e.transform.position.y > player.transform.position.y - detectionRangeY / 2f - enemyRadiusY && e.transform.position.y < player.transform.position.y + detectionRangeY / 2f + enemyRadiusY)
                {
                    detectedEnemies.Add(e);
                }
            }
        }
        
        Block[] allBlocks = GameObject.FindObjectsOfType<Block>();

        for (int i = 0; i < detectedEnemies.Count; i++)
        {
            string[] hitBox = ComputeHitBox(detectedEnemies[i]);
            printInfo.text += detectedEnemies[i].GetType().Name + " " + ComputePosition(detectedEnemies[i]) + " [" + hitBox[0] + ", " + hitBox[1] + ", " + hitBox[2] + ", " + hitBox[3] + "]\n";
        }
    }

    private Vector2 ComputePosition(UnityEngine.Component component)
    {
        if (component is Character || component is Enemy)
        {
            return new Vector2(component.transform.position.x, component.transform.position.y);
        }
        else
        {
            UnityEngine.Debug.LogError("Component for position is not a Character or Enemy");
            return Vector2.zero;
        }
    }

    private string[] ComputeHitBox(UnityEngine.Component component)
    {
        if (component is Character || component is Enemy)
        {
            Vector2 edges = new Vector2(component.GetComponent<Collider2D>().bounds.extents.x, component.GetComponent<Collider2D>().bounds.extents.y);
            string[] hitBox = new string[4];

            hitBox[0] = (component.transform.position.x - edges.x).ToString("0.0");
            hitBox[1] = (component.transform.position.y - edges.y).ToString("0.0");
            hitBox[2] = (component.transform.position.x + edges.x).ToString("0.0");
            hitBox[3] = (component.transform.position.y + edges.y).ToString("0.0");

            return hitBox;
        }
        else
        {
            UnityEngine.Debug.LogError("Component for hitbox is not a Character or Enemy");
            return null;
        }
    }
}
