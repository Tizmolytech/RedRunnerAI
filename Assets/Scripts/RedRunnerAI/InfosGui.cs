using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfosGui : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void drawNetwork(Network n)
    {
        Console.WriteLine("Drawing network");
    }

    public void drawInfos(Population pop, List<Species> species, int nbGeneration)
    {
        Console.WriteLine("Drawing infos");
    }

    public void clearGraphics()
    {
        Console.WriteLine("Clearing graphics");
    }
}
