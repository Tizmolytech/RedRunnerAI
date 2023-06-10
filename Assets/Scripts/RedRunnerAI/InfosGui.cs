using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfosGui : MonoBehaviour
{
    private LineRenderer windowRenderer;
    private Dictionary<Neuron, GameObject> neuronRenderers;
    private Dictionary<Connection, GameObject> connectionRenderers;
    private GameObject drawingObject;
    private bool isDrawingInitialized = false; // to check if the drawing has been initialized
    private bool isDrawingInfos = false; // to check if the drawing has been initialized

    private void initializeObjects()
    {
        drawingObject = GameObject.Find("Drawing");
        neuronRenderers = new Dictionary<Neuron, GameObject>();
        connectionRenderers = new Dictionary<Connection, GameObject>();

        windowRenderer = drawingObject.AddComponent<LineRenderer>();
        windowRenderer.material = new Material(Shader.Find("Sprites/Default"));
        windowRenderer.widthMultiplier = 0.2f;
        windowRenderer.useWorldSpace = false; // to follow camera movements
        windowRenderer.sortingOrder = 1; // to be on top of the other UI elements
    }

    public void drawNetwork(Network n)
    {
        if (!isDrawingInitialized)
        {
            initializeObjects();
            drawWindow();
            drawInputNeurons(n);
            drawHiddenNeurons(n);
            drawOutputNeurons(n);
            drawConnections(n);
            isDrawingInitialized = true;
        }
        else
        {
            updateNeuronColors(n);
            updateConnections(n);
        }
    }

    public void drawInfos(Population pop, List<Species> species, int nbGeneration)
    {
        if (!isDrawingInfos)
        {
            isDrawingInfos = true;
            GameObject textObject = GameObject.Find("Infos Text");
            Text content = textObject.GetComponent<Text>();

            string text = "";
            text += "Generation " + nbGeneration.ToString() + "\n";
            text += "Population " + pop.Count.ToString() + "\n";
            text += "Species " + species.Count.ToString() + "\n";
            text += "Fitness max " + pop[0].Fitness.ToString() + "\n";

            content.text = text;

            content.color = Color.red;
            content.fontSize = 18;
        }
    }
 
    private void clearInfos()
    {
        GameObject textObject = GameObject.Find("Infos Text");
        Text content = textObject.GetComponent<Text>();
        content.text = "";
    }

    public void clearGraphics()
    {
        if (isDrawingInitialized)
        {
            foreach (KeyValuePair<Neuron, GameObject> entry in neuronRenderers)
            {
                Destroy(entry.Value);
            }
            neuronRenderers.Clear();

            Destroy(windowRenderer);

            foreach(KeyValuePair<Connection, GameObject> entry in connectionRenderers)
            {
                Destroy(entry.Value);
            }
            connectionRenderers.Clear();

            isDrawingInitialized = false;
        }
        if (isDrawingInfos)
        {
            isDrawingInfos = false;
            clearInfos();
        }
    }


    private Color getInputColor(double neuronValue)
    {
        return neuronValue switch
        {
            0 => Color.white, //Nothing
            1 => Color.red, //Enemy
            2 => Color.green, //Block
            3 => Color.blue, //RedRunner
            _ => Color.black, //Unknown
        };
    }

    private Color getOtherColor(double neuronValue)
    {
        if (neuronValue < 0)
        {
            return Color.black;
        }
        else if (neuronValue == 0)
        {
            return Color.white;
        }
        else
        {
            return Color.magenta;
        }
    }

    private void drawWindow()
    {
        Vector3 topLeftCorner = new Vector3(-400, 225, 0);
        Vector3 topRightCorner = new Vector3(400, 225, 0);
        Vector3 bottomLeftCorner = new Vector3(-400, -225, 0);
        Vector3 bottomRightCorner = new Vector3(400, -225, 0);

        windowRenderer.positionCount = 4;
        windowRenderer.SetPosition(0, topLeftCorner);
        windowRenderer.SetPosition(1, topRightCorner);
        windowRenderer.SetPosition(2, bottomRightCorner);
        windowRenderer.SetPosition(3, bottomLeftCorner);
        windowRenderer.loop = true; // to close the shape
        windowRenderer.startColor = Color.black;
        windowRenderer.endColor = Color.black;
    }

    private void drawInputNeurons(Network n)    
    {
        float startX = -40f;
        float startY = -350f;

        float neuronSpacingX = 5f;
        float neuronSpacingY = 5f;

        int neuronIndex = 0;

        for (byte x = 0; x < Globals.GRID_H; x++)
        {
            for (byte y = 0; y < Globals.GRID_W; y++)
            {
                drawNeuron(startY + y * neuronSpacingY, startX + x * neuronSpacingX, n.Neurons[neuronIndex]);

                neuronIndex++;
            }
        }
    }

    private void drawNeuron(float x, float y, Neuron neuron)
    {
        GameObject neuronObject = new GameObject("Neuron" + neuron.Id);
        neuronObject.transform.SetParent(GameObject.Find("In-Game Screen").transform, false);
        neuronObject.layer = LayerMask.NameToLayer("UI");
        _ = neuronObject.AddComponent<CanvasRenderer>();
        LineRenderer neuronRenderer = neuronObject.AddComponent<LineRenderer>();
        neuronRenderer.useWorldSpace = false;
        neuronRenderer.alignment = LineAlignment.View;
        neuronRenderer.material = new Material(Shader.Find("Sprites/Default"));
        neuronRenderer.positionCount = 2;
        neuronRenderer.SetPosition(0, new Vector3(x, y, 0));
        neuronRenderer.SetPosition(1, new Vector3(x + 5f, y, 0));
        neuronRenderer.sortingOrder = 1;

        if (neuron.Type == "input")
        {
            neuronRenderer.startColor = getInputColor(neuron.Value);
            neuronRenderer.endColor = getInputColor(neuron.Value);
        }
        else
        {
            neuronRenderer.startColor = getOtherColor(neuron.Value);
            neuronRenderer.endColor = getOtherColor(neuron.Value);
        }

        neuronRenderer.widthMultiplier = 0.2f;

        neuronRenderers.Add(neuron, neuronObject);
    }

    private void updateNeuronColors(Network network)
    {
        foreach(KeyValuePair<Neuron, GameObject> neurony in neuronRenderers)
        {
            if (neurony.Key.Type == "input")
            {
                neurony.Value.GetComponent<LineRenderer>().startColor = getInputColor(neurony.Key.Value);
                neurony.Value.GetComponent<LineRenderer>().endColor = getInputColor(neurony.Key.Value);
            }
            else
            {
                neurony.Value.GetComponent<LineRenderer>().startColor = getOtherColor(neurony.Key.Value);
                neurony.Value.GetComponent<LineRenderer>().endColor = getOtherColor(neurony.Key.Value);
            }
        }
    }

    private void drawHiddenNeurons(Network network)
    {
        int numberHiddenNeurons = network.Neurons.Count - Globals.GRID_W * Globals.GRID_H - Globals.NB_OUTPUTS;

        float startX = 0f;
        float startY = -50f;

        float neuronSpacingX = 10f;
        float neuronSpacingY = 5f;

        uint neuronsPerLine = 20;

        int neuronIndex = Globals.GRID_W * Globals.GRID_H;

        for (uint i = 0; i < numberHiddenNeurons; i++)
        {
            uint lineIndex = i / neuronsPerLine;
            uint columnIndex = i % neuronsPerLine;

            drawNeuron(startY + lineIndex * neuronSpacingY, startX - columnIndex * neuronSpacingX, network.Neurons[neuronIndex]);

            neuronIndex++;
        }
    }

    private void drawOutputNeurons(Network network)
    {
        float startX = -25f;
        float startY = 350f;

        byte neuronSpacingX = 10;

        int neuronIndex = network.Neurons.Count - Globals.NB_OUTPUTS;

        for (byte x = 0; x < Globals.NB_OUTPUTS; x++)
        {
            drawNeuron(startY, startX + x * neuronSpacingX, network.Neurons[neuronIndex]);
            neuronIndex++;
        }
    }

    private void drawConnections(Network network)
    {
        foreach (Connection connection in network.Connections)
        {
            Neuron fromNeuron = network.Neurons[connection.Input - 1];
            Neuron toNeuron = network.Neurons[connection.Output - 1];

            if (neuronRenderers.ContainsKey(fromNeuron) && neuronRenderers.ContainsKey(toNeuron))
            {
                GameObject fromObject = neuronRenderers[fromNeuron];
                GameObject toObject = neuronRenderers[toNeuron];

                GameObject connectionObject = new GameObject("Connection");
                connectionObject.transform.SetParent(GameObject.Find("In-Game Screen").transform, false);
                connectionObject.layer = LayerMask.NameToLayer("UI");
                _ = connectionObject.AddComponent<CanvasRenderer>();

                LineRenderer connectionRenderer = connectionObject.AddComponent<LineRenderer>();
                connectionRenderer.useWorldSpace = false;
                connectionRenderer.material = new Material(Shader.Find("Sprites/Default"));
                connectionRenderer.startColor = new Color(0f, 0f, 0f, 0f);
                connectionRenderer.endColor = new Color(0f, 0f, 0f, 0f);
                connectionRenderer.sortingOrder = 2;
                connectionRenderer.widthMultiplier = 0.15f;
                connectionRenderer.SetPosition(0, fromObject.GetComponent<LineRenderer>().GetPosition(0));
                connectionRenderer.SetPosition(1, toObject.GetComponent<LineRenderer>().GetPosition(0));

                connectionRenderers.Add(connection, connectionObject);
            }
        }
    }

    private void updateConnections(Network network)
    {
        foreach (KeyValuePair<Connection, GameObject> connection in connectionRenderers)
        {
            if (connection.Key.On == true)
            {
                connection.Value.GetComponent<LineRenderer>().startColor = Color.gray;
                connection.Value.GetComponent<LineRenderer>().endColor = Color.gray;
            }
            else
            {
                connection.Value.GetComponent<LineRenderer>().startColor = new Color(0f, 0f, 0f, 0f);
                connection.Value.GetComponent<LineRenderer>().endColor = new Color(0f, 0f, 0f, 0f);
            }
        }
    }
}
