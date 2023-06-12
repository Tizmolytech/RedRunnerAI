using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfosGui : MonoBehaviour
{
    private LineRenderer windowRenderer;
    private Dictionary<int, GameObject> neuronRenderers;
    private List<GameObject> connectionRenderers;
    private GameObject drawingObject;
    private bool isDrawingInitialized = false; // to check if the drawing has been initialized
    private bool isDrawingInfos = false; // to check if the drawing has been initialized
    private GameObject outputUp;
    private GameObject outputDown;
    private GameObject outputLeft;
    private GameObject outputRight;

    private void initializeObjects()
    {
        drawingObject = GameObject.Find("Drawing");
        neuronRenderers = new Dictionary<int, GameObject>();
        connectionRenderers = new List<GameObject>();
        outputDown = GameObject.Find("Down");
        outputUp = GameObject.Find("Up");
        outputRight = GameObject.Find("Right");
        outputLeft = GameObject.Find("Left");

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
            drawInputSpeedNeurons(n);
            drawHiddenNeurons(n);
            drawOutputNeurons(n);
            drawConnections(n);
            drawOutputKeys();
            isDrawingInitialized = true;
        }
        else
        {
            updateNeuronColors(n);
            updateConnections(n);
        }
    }

    public void drawInfos(Population pop, List<Species> species, int nbGeneration, double fitnessMax, Network network, int idNetwork)
    {
        string text = "";
        text += "Generation " + nbGeneration.ToString() + "\n";
        text += "Population " + pop.Count.ToString() + "\n";
        text += "Species " + species.Count.ToString() + "\n";
        text += "Fitness max " + fitnessMax.ToString() + "\n";
        text += "Current fitness " + network.Fitness.ToString() + "\n";
        text += "Current network " + idNetwork.ToString();
        isDrawingInfos = true;
        GameObject textObject = GameObject.Find("Infos Text");
        Text content = textObject.GetComponent<Text>();

        if(content.text != text)
            content.text = text;

        content.color = Color.black;
        content.fontSize = 18;
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
            foreach (KeyValuePair<int, GameObject> entry in neuronRenderers)
            {
                Destroy(entry.Value);
            }
            neuronRenderers.Clear();

            Destroy(windowRenderer);
            
            foreach (GameObject connection in connectionRenderers)
            {
                Destroy(connection);
            }

            connectionRenderers.Clear();

            outputUp.GetComponent<Text>().text = "";
            outputDown.GetComponent<Text>().text = "";
            outputLeft.GetComponent<Text>().text = "";
            outputRight.GetComponent<Text>().text = ""; 

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
            -1 => Color.red, //Enemy
            1 => Color.green, //Block
            2 => Color.blue, //RedRunner
            _ => Color.black, //Unknown
        };
    }

    private Color getOtherColor(double neuronValue)
    {
        if (neuronValue < 1)
            return Color.black;
        else
            return Color.white;
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
        float startY = -390f;

        float neuronSpacingX = 5f;
        float neuronSpacingY = 5f;

        int neuronIndex = 0;

        for (byte x = 0; x < Globals.GRID_H; x++)
        {
            for (byte y = 0; y < Globals.GRID_W; y++)
            {
                drawNeuron(startY + y * neuronSpacingY, startX + x * neuronSpacingX, n.Neurons[neuronIndex], 0.2f);

                neuronIndex++;
            }
        }
    }

    private void drawNeuron(float x, float y, Neuron neuron, float size)
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
        neuronRenderer.SetPosition(1, new Vector3(x + 25 * size, y, 0));
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

        neuronRenderer.widthMultiplier = size;

        neuronRenderers.Add(neuron.Id, neuronObject);
    }

    private void updateNeuronColors(Network network)
    {
        foreach(KeyValuePair<int, GameObject> neurony in neuronRenderers)
        {
            if (neurony.Key >= 1 && neurony.Key < Globals.GRID_H * Globals.GRID_W)
            {
                neurony.Value.GetComponent<LineRenderer>().startColor = getInputColor(network.Neurons[neurony.Key - 1].Value);
                neurony.Value.GetComponent<LineRenderer>().endColor = getInputColor(network.Neurons[neurony.Key - 1].Value);
            }
            else
            {
                neurony.Value.GetComponent<LineRenderer>().startColor = getOtherColor(network.Neurons[neurony.Key - 1].Value);
                neurony.Value.GetComponent<LineRenderer>().endColor = getOtherColor(network.Neurons[neurony.Key - 1].Value);
            }
        }
    }

    private void drawHiddenNeurons(Network network)
    {
        int numberHiddenNeurons = Globals.NB_INPUTS + Globals.NB_OUTPUTS;

        float startX = -50f;
        float startY = -50f;

        float neuronSpacingX = 7f;
        float neuronSpacingY = 7f;

        uint neuronsPerLine = 60;

        int neuronIndex = numberHiddenNeurons;

        for (uint i = 0; i < network.Neurons.Count - numberHiddenNeurons; i++)
        {
            uint lineIndex = i / neuronsPerLine;
            uint columnIndex = i % neuronsPerLine;

            drawNeuron(startY + lineIndex * neuronSpacingY, startX - columnIndex * neuronSpacingX, network.Neurons[neuronIndex], 0.2f);
            neuronIndex++;
        }
    }

    private void drawOutputNeurons(Network network)
    {
        float startX = -25f;
        float startY = 375f;

        byte neuronSpacingX = 25;

        int neuronIndex = Globals.NB_INPUTS;

        for (byte x = 0; x < Globals.NB_OUTPUTS; x++)
        {
            drawNeuron(startY, startX + x * neuronSpacingX, network.Neurons[neuronIndex], 0.5f);
            neuronIndex++;
        }
    }

    private void drawInputSpeedNeurons(Network network)
    {
        float startX = -100f;
        float startY = -300f;

        float neuronSpacingY = 10f;

        int neuronIndex = Globals.GRID_H * Globals.GRID_W;

        for (byte x = 0; x < Globals.MORE_INPUTS; x++)
        {
            drawNeuron(startY, startX + x * neuronSpacingY, network.Neurons[neuronIndex], 0.2f);
            neuronIndex++;
        }
    }

    private void drawConnections(Network network)
    {
        foreach (Connection connection in network.Connections)
        {
            int fromNeuron = connection.Input;
            int toNeuron = connection.Output;

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
                connectionRenderer.widthMultiplier = 0.05f;
                connectionRenderer.SetPosition(0, fromObject.GetComponent<LineRenderer>().GetPosition(0));
                connectionRenderer.SetPosition(1, toObject.GetComponent<LineRenderer>().GetPosition(0));

                connectionRenderers.Add(connectionObject);
            }
        }
    }

    private void drawOutputUp()
    {
        string text = "Up";
        outputUp.GetComponent<Text>().text = text;
        outputUp.GetComponent<Text>().color = Color.black;
    }

    private void drawOutputDown()
    {
        string text = "Down";
        outputDown.GetComponent<Text>().text = text;
        outputDown.GetComponent<Text>().color = Color.black;
    }

    private void drawOutputLeft()
    {
        string text = "Left";
        outputLeft.GetComponent<Text>().text = text;
        outputLeft.GetComponent<Text>().color = Color.black;
    }

    private void drawOutputRight()
    {
        string text = "Right";
        outputRight.GetComponent<Text>().text = text;
        outputRight.GetComponent<Text>().color = Color.black;
    }

    private void drawOutputKeys()
    {
        drawOutputDown();
        drawOutputLeft();
        drawOutputRight();
        drawOutputUp();
    }

    private void updateConnections(Network network)
    {
        for(int i = 0; i < connectionRenderers.Count; i++)
        {
            if (network.Connections[i].On)
            {
                if (network.Connections[i].Weight > 0)
                {
                    connectionRenderers[i].GetComponent<LineRenderer>().startColor = Color.white;
                    connectionRenderers[i].GetComponent<LineRenderer>().endColor = Color.white;
                }
                else
                {
                    connectionRenderers[i].GetComponent<LineRenderer>().startColor = Color.black;
                    connectionRenderers[i].GetComponent<LineRenderer>().endColor = Color.black;
                }
            }
            else
            {
                connectionRenderers[i].GetComponent<LineRenderer>().startColor = new Color(0f, 0f, 0f, 0f);
                connectionRenderers[i].GetComponent<LineRenderer>().endColor = new Color(0f, 0f, 0f, 0f);
            }
        }
    }
}
