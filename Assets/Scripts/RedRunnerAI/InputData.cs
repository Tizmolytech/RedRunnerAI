using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using RedRunner.Enemies;
using RedRunner.Characters;
using UnityEngine.UI;

public class InputData : MonoBehaviour
{
    private byte[,] data;
    private Character player;
    private Camera cam;
    private Vector2 detectionRange;
    private Dictionary<Enemy, Vector4> detectedEnemies;
    private Dictionary<GameObject, Vector4> detectedGround;
    private RawImage gridImage;
    private readonly float smallestObjectX = 1.02f; // width of semi circular saw
    private readonly float smallestObjectY = 0.86f; // height of a spike up
    private readonly string filePath = Environment.GetEnvironmentVariable("USERPROFILE") + "\\OneDrive\\Bureau\\grid.txt";

    public Color[] colorMap = new Color[4]; // Array of colors corresponding to different object types

    private void Start()
    {
        player = GameObject.Find("RedRunner").GetComponent<Character>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        gridImage = GameObject.Find("Minimap").GetComponent<RawImage>();
        detectionRange = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        data = InitDataSize();
        colorMap[0] = new Color(1, 1, 1, 0.5f);
        colorMap[1] = Color.red;
        colorMap[2] = Color.green;
        colorMap[3] = Color.blue;
    }

    private void Update()
    {
        detectedEnemies = DetectObjects(FindObjectsOfType<Enemy>());
        detectedGround = DetectGround(GameObject.FindGameObjectsWithTag("Ground"));
        ComputeGrid();
        DisplayGrid();
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
        ResetGrid();

        foreach (KeyValuePair<Enemy, Vector4> enemy in detectedEnemies)
        {
            ComputePositionInGrid(enemy.Value, 1);
        }

        foreach (KeyValuePair<GameObject, Vector4> ground in detectedGround)
        {
            ComputePositionInGrid(ground.Value, 2);
        }

        Vector4 playerPosition = ComputeHitBox(player.GetComponent<Collider2D>());
        ComputePositionInGrid(playerPosition, 3);

        DisplayGrid();
    }

    private void ComputePositionInGrid(Vector4 hitBox, byte objectType)
    {
        Vector4 gridPosition = CorrectPosition(CorrectValue(new Vector4((hitBox.x - cam.transform.position.x + detectionRange.x) / smallestObjectX,
            (hitBox.y - cam.transform.position.y + detectionRange.y) / smallestObjectY, (hitBox.z - cam.transform.position.x + detectionRange.x)
            / smallestObjectX, (hitBox.w - cam.transform.position.y + detectionRange.y) / smallestObjectY)));

        for (int i = (int)gridPosition.y; i < (int)gridPosition.w; i++)
        {
            for (int j = (int)gridPosition.x; j < (int)gridPosition.z; j++)
            {
                data[i, j] = objectType;
            }
        }
    }

    private Vector4 CorrectPosition(Vector4 hitBox)
    {
        return new Vector4(CustomRound(hitBox.x), CustomRound(hitBox.y), CustomRound(hitBox.z), CustomRound(hitBox.w));
    }

    private void SaveGrid(byte[,] dataGrid)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int loopWidth = dataGrid.GetLength(0) - 1; loopWidth >= 0; loopWidth--)
            {
                for (int loopHeight = 0; loopHeight < dataGrid.GetLength(1); loopHeight++)
                {
                    writer.Write(dataGrid[loopWidth, loopHeight] + " ");
                }
                writer.WriteLine();
            }
        }
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

    private byte[,] InitDataSize()
    {
        float rawSizeX = (2 * detectionRange.x) / smallestObjectX;
        float rawSizeY = (2 * detectionRange.y) / smallestObjectY;

        return new byte[CustomRound(rawSizeY), CustomRound(rawSizeX)];
    }

    private Vector4 CorrectValue(Vector4 hitBox)    
    {
        if (hitBox.x < 0)
            hitBox.x = 0;
        if (hitBox.y < 0)
            hitBox.y = 0;
        if (hitBox.z < 0)
            hitBox.z = 0;
        if (hitBox.w < 0)
            hitBox.w = 0;
        if (hitBox.x >= data.GetLength(1))
            hitBox.x = data.GetLength(1) - 1;
        if (hitBox.y >= data.GetLength(0))
            hitBox.y = data.GetLength(0) - 1;
        if (hitBox.z >= data.GetLength(1))
            hitBox.z = data.GetLength(1) - 1;
        if (hitBox.w >= data.GetLength(0))
            hitBox.w = data.GetLength(0) - 1;

        return hitBox;
    }

    private byte CustomRound(float value)
    {
        float result = (float)(value - Math.Truncate(value));

        if (result >= 0 && result < 0.5)
        {
            value = (byte)Math.Truncate(value);
        }
        else if (result >= 0.5 && result < 1)
        {
            value = (byte)Math.Truncate(value) + 1;
        }

        return (byte)value;
    }

    private void DisplayGrid()
    {
        int width = data.GetLength(1) - 1;
        int height = data.GetLength(0) - 1;

        Texture2D texture = new(width, height);
        texture.filterMode = FilterMode.Point;

    
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte objectType = data[y, x];
                Color color = colorMap[objectType];
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        gridImage.texture = texture;
    }
}