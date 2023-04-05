using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RedRunner.Characters;
using UnityEngine.Networking;

public class CurrentPosition : MonoBehaviour
{
    public TextMeshProUGUI printInfo;
    public Character player;
    private Vector2 previousVelocity;

    void Start()
    {
        printInfo = GameObject.Find("Information Text").GetComponent<TextMeshProUGUI>();
        player = GameObject.FindWithTag("Player").GetComponent<Character>();
        previousVelocity = Vector2.zero;
    }

    void Update()
    {
        Vector2 currentVelocity = player.GetComponent<Rigidbody2D>().velocity;
        Vector2 acceleration = (currentVelocity - previousVelocity) / Time.deltaTime;
        previousVelocity = currentVelocity;

        printInfo.text = "Current Position: " + player.transform.position + "\n" +
        "Current Velocity: " + currentVelocity + "\n" +
        "Current Acceleration: " + acceleration + "\n";
    }
}

