using RedRunner;
using RedRunner.Characters;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/**
 * Class to make work the genetic algorithm, train it, and save the model
**/
public class Manager : MonoBehaviour
{
    private Population population;
    private List<Species> species;
    private int idPopulation = 0;
    private InfosGui ig = new InfosGui();
    private double prevPosX = 0;
    private Character character;
    private int nbFrames = 0;
    private int nbFrameStop = 0;
    private double fitnessInit = 0;
    private double fitnessMax = 0;
    private InputData inputData;
    private Network testNetwork;
    private string mode = "train";
    private bool drawInfos = false;
    private bool drawNetwork = false;

    private void loadGen(int genToLoad)
    {
        testNetwork = new Network();
        
        Globals.numberGeneration = genToLoad;
        if(genToLoad == 0)
        {
            population = new Population();
            AllPops.bestNetwork = new Network(testNetwork);
            return;
        }

        testNetwork.deserialize(genToLoad);
        AllPops.bestNetwork = new Network(testNetwork);
        population = new Population();
        population.deserialize(genToLoad);
        fitnessMax = testNetwork.Fitness;
        testNetwork.Fitness = 0;
    }

    private int getGenToLoad()
    {
        int genToLoad = 0;
        string path = "genToLoad.txt";
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            genToLoad = int.Parse(lines[0]);
            mode = lines[1];
            Globals.NB_INDIVIDUAL_POPULATION = int.Parse(lines[2]);
        }
        return genToLoad;
    }

    void Start()
    {
        Application.targetFrameRate = 30;
        loadGen(getGenToLoad());
        character = GameObject.Find("RedRunner").GetComponent<Character>();
        inputData = GameObject.Find("EventSystem").GetComponent<InputData>();

        for (int i = 0; i < population.Count; i++)
            population[i].mutate();

        for (int i = 1; i < population.Count; i++)
        {
            population[i] = new Network(population[0]);
            population[i].mutate();
        }

        species = population.sortPopulation();

        population = AllPops.newGeneration(population, ref species);

        prevPosX = character.transform.position.x;
    }

    void Update()
    {
        if (!GameManager.Singleton.gameStarted || !GameManager.Singleton.gameRunning)
            return;
        if (Input.GetKeyDown(KeyCode.I))
        {
            drawInfos = !drawInfos;
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            drawNetwork = !drawNetwork;
        }
        switch (mode)
        {
            case "train":
                train();
                break;
            case "test":
                test();
                break;
            case "play":
                play();
                break;
            default:
                break;
        }
    }

    private void play()
    {
        inputData.GetDatasOneLine();
        if (Input.GetAxis("Horizontal") < 0f) //q
            character.Move(-1f);
        else if (Input.GetAxis("Horizontal") > 0f) //d
            character.Move(1f);
        else
            character.Move(0f);
        if (Input.GetKeyDown(KeyCode.W)) //z
            character.Jump();
        if (Input.GetKeyDown(KeyCode.S)) //s
            character.Roll();
    }

    private void test()
    {
        if (drawNetwork)
            ig.drawNetwork(testNetwork);
        if (!drawNetwork)
            ig.clearGraphics();
        testNetwork.update(ref prevPosX, inputData);
        testNetwork.feedForward();
        testNetwork.applyOutput();
    }

    private void train()
    {
        if(drawInfos)
            ig.drawInfos(population, species, Globals.numberGeneration, fitnessMax, population[idPopulation], idPopulation);
        if (drawNetwork)
            ig.drawNetwork(population[idPopulation]);
        if(!drawInfos && !drawNetwork)
            ig.clearGraphics();


        double prevFitness = population[idPopulation].Fitness;

        population[idPopulation].update(ref prevPosX, inputData);
        population[idPopulation].feedForward();
        population[idPopulation].applyOutput();

        if (nbFrames == 0)
            fitnessInit = population[idPopulation].Fitness;

        nbFrames++;

        if (fitnessMax < population[idPopulation].Fitness)
            fitnessMax = population[idPopulation].Fitness;

        if (prevFitness == population[idPopulation].Fitness)
        {
            nbFrameStop++;
            int nbFramesReset = Globals.NB_FRAME_RESET_BASE;

            if (fitnessInit != population[idPopulation].Fitness && !character.IsDead.Value)
                nbFramesReset = Globals.NB_FRAME_RESET_PROGRESS;

            if (nbFrameStop > nbFramesReset)
            {
                RReset();
            }

        }
        else
            nbFrameStop = 0;
        //double Value = population[idPopulation].Neurons[Globals.NB_INPUTS + Globals.NB_OUTPUTS - 1].Value;

        if (character.IsDead.Value)
            RReset();
    }

    public void RReset()
    {
        ig.clearGraphics();
        nbFrameStop = 0;
        nbFrames = 0;
        GameManager.Singleton.Reset();
        var ingameScreen = UIManager.Singleton.GetUIScreen(UIScreenInfo.IN_GAME_SCREEN);
        UIManager.Singleton.OpenScreen(ingameScreen);
        GameManager.Singleton.StartGame();
        idPopulation++;
        if (idPopulation >= population.Count)
        {
            idPopulation = 0;
            species = population.sortPopulation();
            population = AllPops.newGeneration(population, ref species);
            nbFrames = 0;
            fitnessInit = 0;
        }
        character.Reset();
        prevPosX = character.transform.position.x;
    }

    private string getLabel()
    {
        string label = "Generation: " + Globals.numberGeneration.ToString() + "\t Fitness max: " + fitnessMax + "\n Current individual:\n" + "id: " + idPopulation + "/" + (population.Count - 1);// + " Neurons: "; 
        //foreach(Neuron n in population[idPopulation].Neurons)
        //    label += "Id: " + n.Id + "Value: " + n.Value + "\n";
        //label += "\nConnections: ";
        //foreach(Connection c in population[idPopulation].Connections)
        //    label += "In: " + c.Input + "Out: " + c.Output + "Weight: " + c.Weight + "\n";

        return label;
    }

    private string getZQSD()
    {
        string label = "z " + population[idPopulation].Neurons[Globals.NB_INPUTS + Globals.NB_OUTPUTS - 4].Value.ToString("0.##")
            + "q " + population[idPopulation].Neurons[Globals.NB_INPUTS + Globals.NB_OUTPUTS - 3].Value.ToString("0.##")
            + "s " + population[idPopulation].Neurons[Globals.NB_INPUTS + Globals.NB_OUTPUTS - 2].Value.ToString("0.##")
            + "d " + population[idPopulation].Neurons[Globals.NB_INPUTS + Globals.NB_OUTPUTS - 1].Value.ToString("0.##");

        return label;
    }
}