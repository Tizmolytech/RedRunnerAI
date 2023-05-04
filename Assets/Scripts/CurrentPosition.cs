using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RedRunner.Characters;
using RedRunner.Enemies;
using RedRunner.Collectables;
using System.Reflection;

public class CurrentPosition : MonoBehaviour
{
    private const int hudOffset = 260;
    private TextMeshProUGUI printInfo;
    private Character player;
    private Vector2 previousVelocity;
    private Camera cam;
    private Vector2 detectionRange;
    private bool isTextVisible = false;
    private UnityEngine.UI.Image image;

    void Start()
    {
        printInfo = GameObject.Find("Information Text").GetComponent<TextMeshProUGUI>();
        printInfo.enabled = isTextVisible;
        player = GameObject.Find("RedRunner").GetComponent<Character>();
        previousVelocity = Vector2.zero;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        detectionRange = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        image = GameObject.Find("Information background").GetComponent<Image>();
        image.enabled = isTextVisible;
        image.rectTransform.anchoredPosition = new Vector2(detectionRange.x - hudOffset, detectionRange.y * 0.5f);
    }

    void Update()
    {
        Vector2 currentVelocity = player.GetComponent<Rigidbody2D>().velocity;

        
        printInfo.text = "<align=\"center\"><u>Information</u></align>\nCurrent Position: (" + player.transform.position.x.ToString("N2") + ", " + player.transform.position.y.ToString("N2") + ")\n" +
        "Current Velocity: " + currentVelocity + "\n" +
        "Current Acceleration: " + (currentVelocity - previousVelocity) / Time.deltaTime + "\n";

        printObjects(detectObjects(FindObjectsOfType<Coin>()));
        printObjects(detectObjects(FindObjectsOfType<Enemy>()));
        previousVelocity = currentVelocity;

        if (Input.GetKeyDown(KeyCode.I))
        {
            isTextVisible = !isTextVisible;
            printInfo.enabled = isTextVisible;
            image.enabled = isTextVisible;
        }
    }

    private Dictionary<T, Vector4> detectObjects<T>(T[] allObjects) where T : MonoBehaviour
    {
        Dictionary<T, Vector4> result = new Dictionary<T, Vector4>();

        foreach (T obj in allObjects)
        {
            Vector4 hitBox = ComputeHitBox(obj);

            if (hitBox.z > cam.transform.position.x - detectionRange.x && hitBox.x < cam.transform.position.x + detectionRange.x && hitBox.w > cam.transform.position.y - detectionRange.y && hitBox.y < cam.transform.position.y + detectionRange.y)
            {
               result.Add(obj, hitBox);
            }
        }
        
        return result;
    }

    private void printObjects<T>(Dictionary<T, Vector4> detectedObjects) where T : MonoBehaviour
    {
        foreach (KeyValuePair<T, Vector4> entry in detectedObjects)
        {
            printInfo.text += entry.Key.GetType().Name + " [(" + entry.Value.x.ToString("N2") + ", " + entry.Value.y.ToString("N2") + "), (" + entry.Value.z.ToString("N2") + ", " + entry.Value.w.ToString("N2") + ")]\n";
        }
    }

    private Vector4 ComputeHitBox(Component component)
    {
        Vector3 exts = component.GetComponent<Collider2D>().bounds.extents;

        return new Vector4(component.transform.position.x - exts.x, component.transform.position.y - exts.y, component.transform.position.x + exts.x, component.transform.position.y + exts.y);
    }
}
