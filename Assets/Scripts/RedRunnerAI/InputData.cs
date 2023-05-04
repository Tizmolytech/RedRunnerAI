using System.Collections.Generic;
using RedRunner.Characters;
using RedRunner.Enemies;
using RedRunner.Collectables;
using RedRunner.TerrainGeneration;
using UnityEngine;
/**
 * Class to prepare data
 * Data is a grid of the screen and each cell represents an object such as an enemy, a player, a ground block, etc.
 * The goal of this class is to "prepare" the data so that it can be used by the neural network
 * The data looks like a grid with bytes representing the objects
**/
public class InputData : MonoBehaviour
{
	private byte[,] data;
	private uint numberInputs;
    private Character player;
    private Vector2 previousVelocity;
    private Camera cam;
    private Vector2 detectionRange;
    private Dictionary<Coin, Vector4> detectedCoins;
    private Dictionary<Enemy, Vector4> detectedEnemies;
    private Dictionary<Block, Vector4> detectedBlocks;
    private const byte smallestObject = 4; //Coin which is 0.4 square

	void Start()
	{
        player = GameObject.Find("RedRunner").GetComponent<Character>();
        previousVelocity = Vector2.zero;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        detectionRange = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        data = new byte[(int)detectionRange.x / smallestObject, (int)detectionRange.y / smallestObject];
    }

	void Update()
	{
        Vector2 currentVelocity = player.GetComponent<Rigidbody2D>().velocity;
        previousVelocity = currentVelocity;

        detectedCoins = detectObjects(FindObjectsOfType<Coin>());
        detectedEnemies = detectObjects(FindObjectsOfType<Enemy>());
        detectedBlocks = detectObjects(FindObjectsOfType<Block>());
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

    private Vector4 ComputeHitBox(Component component)
    {
        Vector3 exts = component.GetComponent<Collider2D>().bounds.extents;

        return new Vector4(component.transform.position.x - exts.x, component.transform.position.y - exts.y, component.transform.position.x + exts.x, component.transform.position.y + exts.y);
    }

    private void computeGrid()
    {

    }
}