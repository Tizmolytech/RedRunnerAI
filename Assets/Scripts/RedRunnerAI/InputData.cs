using System.Collections.Generic;
using RedRunner.Characters;
using RedRunner.Enemies;
using RedRunner.Collectables;
using UnityEngine;
using System.IO;
using System;
/**
* Class to prepare data
* Data is a grid of the screen and each cell represents an object such as an enemy, a player, a ground block, etc.
* The goal of this class is to "prepare" the data so that it can be used by the neural network
* The data looks like a grid with bytes representing the objects
**/
public class InputData : MonoBehaviour
{
	private int[,] data;
    private Character player;
    private Vector2 previousVelocity;
    private Camera cam;
    private Vector2 detectionRange; 
    private Dictionary<Coin, Vector4> detectedCoins;
    private Dictionary<Enemy, Vector4> detectedEnemies;
    private Dictionary<GameObject, Vector4> detectedGround;
    private float smallestObject =1f;
    private string filePath = Environment.GetEnvironmentVariable("USERPROFILE") + "\\grid.txt";
	void Start()
	{
        player = GameObject.Find("RedRunner").GetComponent<Character>();
        previousVelocity = Vector2.zero;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        detectionRange = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        data = new int[Mathf.CeilToInt(2 * detectionRange.y / smallestObject), Mathf.CeilToInt(2 * detectionRange.x / smallestObject)];
    }

	void Update()
	{

    }

    private Dictionary<T, Vector4> DetectObjects<T>(T[] allObjects) where T : MonoBehaviour
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

    private Dictionary<GameObject, Vector4> DetectGround(GameObject[] allGrounds)
    {
        Dictionary<GameObject, Vector4> result = new Dictionary<GameObject, Vector4>();

        foreach (GameObject obj in allGrounds)
        {
            if (obj.GetComponent<Collider2D>())
            {
                Vector4 hitBox = ComputeHitBox(obj.GetComponent<Collider2D>());

                if (hitBox.z > cam.transform.position.x - detectionRange.x && hitBox.x < cam.transform.position.x + detectionRange.x && hitBox.w > cam.transform.position.y - detectionRange.y && hitBox.y < cam.transform.position.y + detectionRange.y)
                {
                    result.Add(obj, hitBox);
                }
            }
        }

        return result;
    }

    private Vector4 ComputeHitBox(Component component)
    {
        Vector3 exts = component.GetComponent<Collider2D>().bounds.extents;

        return new Vector4(component.transform.position.x - exts.x, component.transform.position.y - exts.y, component.transform.position.x + exts.x, component.transform.position.y + exts.y);
    }

    private void ComputeGrid()
    {
        /**
         * If in the cell, there is nothing, the value is 0
         * If in the cell, there is an enemy, the value is 1
         * If in the cell, there is a coin, the value is 2
         * If in the cell, there is a ground block, the value is 3
         * If in the cell, there is the player, the value is 4
        **/
        ResetGrid();

        foreach (KeyValuePair<Coin, Vector4> coin in detectedCoins)
        {
            FillObjectsInGrid(CorrectPosition(new Vector4(coin.Value.x - cam.transform.position.x + detectionRange.x, coin.Value.y - cam.transform.position.y + detectionRange.y, coin.Value.z - cam.transform.position.x + detectionRange.x, coin.Value.w - cam.transform.position.y + detectionRange.y)), 2);
        }

        foreach (KeyValuePair<Enemy, Vector4> enemy in detectedEnemies)
        {            
            FillObjectsInGrid(CorrectPosition(new Vector4(enemy.Value.x - cam.transform.position.x + detectionRange.x, enemy.Value.y - cam.transform.position.y + detectionRange.y, enemy.Value.z - cam.transform.position.x + detectionRange.x, enemy.Value.w - cam.transform.position.y + detectionRange.y)), 1);
        }

        foreach (KeyValuePair<GameObject, Vector4> ground in detectedGround)
        {
            FillObjectsInGrid(CorrectPosition(new Vector4(ground.Value.x - cam.transform.position.x + detectionRange.x, ground.Value.y - cam.transform.position.y + detectionRange.y, ground.Value.z - cam.transform.position.x + detectionRange.x, ground.Value.w - cam.transform.position.y + detectionRange.y)), 3);
        }

        Vector4 playerPosition = ComputeHitBox(player.GetComponent<Collider2D>());
        FillObjectsInGrid(CorrectPosition(new Vector4(playerPosition.x - cam.transform.position.x + detectionRange.x, playerPosition.y - cam.transform.position.y + detectionRange.y, playerPosition.z - cam.transform.position.x + detectionRange.x, playerPosition.w - cam.transform.position.y + detectionRange.y)), 4);
    }

    private void SaveGrid()
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int loopWidth = data.GetLength(0) -1; loopWidth >= 0; loopWidth--)
            {
                for (int loopHeight = 0; loopHeight < data.GetLength(1); loopHeight++)
                {
                    writer.Write(data[loopWidth, loopHeight] + " ");
                }
                writer.WriteLine();
            }
        }
    }

    private void FillObjectsInGrid(Vector4 position, byte objectType)
    {
        for (byte loopWidth = (byte)Math.Truncate(position.x); loopWidth < Math.Truncate(position.z) + 1; loopWidth++)
        {
            for (byte loopHeight = (byte)Math.Truncate(position.y); loopHeight < Math.Truncate(position.w) + 1; loopHeight++)
            {
                data[loopHeight, loopWidth] = objectType;
            }
        }
    }

    private Vector4 CorrectPosition(Vector4 position)
    {
        position.x = Mathf.Clamp(position.x / smallestObject, 0, data.GetLength(1) - 1);
        position.y = Mathf.Clamp(position.y / smallestObject, 0, data.GetLength(0) - 1);
        position.z = Mathf.Clamp(position.z / smallestObject, 0, data.GetLength(1) - 1);
        position.w = Mathf.Clamp(position.w / smallestObject, 0, data.GetLength(0) - 1);

        return position;
    }
    private void ResetGrid()
    {
        for (ushort loopWidth = 0; loopWidth < data.GetLength(0); loopWidth++)
        {
            for (ushort loopHeight = 0; loopHeight < data.GetLength(1); loopHeight++)
            {
                data[loopWidth, loopHeight] = 0;
            }
        }
    }

    public int[,] GetDatas()
    {
        return data;
    }

    public int[] GetDatasOneLine()
    {
        Vector2 currentVelocity = player.GetComponent<Rigidbody2D>().velocity;
        previousVelocity = currentVelocity;

        detectedCoins = DetectObjects(FindObjectsOfType<Coin>());
        detectedEnemies = DetectObjects(FindObjectsOfType<Enemy>());
        detectedGround = DetectGround(GameObject.FindGameObjectsWithTag("Ground"));
        ComputeGrid();
        //SaveGrid();
        int[] result = new int[data.GetLength(0) * data.GetLength(1) + 2];

        for (int loopWidth = 0; loopWidth < data.GetLength(0); loopWidth++)
        {
            for (int loopHeight = 0; loopHeight < data.GetLength(1); loopHeight++)
            {
                result[loopWidth * data.GetLength(1) + loopHeight] = data[loopWidth, loopHeight];
            }
        }
        result[result.Length - 2] = (int)player.Speed.x;
        result[result.Length - 1] = (int)player.Speed.y;
        return result;
    }
}