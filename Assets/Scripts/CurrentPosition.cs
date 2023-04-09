using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RedRunner.Characters;
using RedRunner.Enemies;
using RedRunner.Utilities;
using RedRunner.TerrainGeneration;

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
        "Current Acceleration: " + acceleration + "\n";

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
            printInfo.text += detectedEnemies[i].GetType().Name + " " + detectedEnemies[i].transform.position + "\n";
        }
    }
}
