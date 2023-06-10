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
    private AllPops allPops = new AllPops();
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

    private void loadGen(int genToLoad)
    {
        testNetwork = new Network();
        
        Globals.numberGeneration = genToLoad;
        if(genToLoad == 0)
        {
            population = new Population();
            return;
        }

        testNetwork.deserialize(genToLoad);
        population = new Population(testNetwork);
        testNetwork.Fitness = 0;
        fitnessMax = population[0].Fitness;
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

        population = allPops.newGeneration(population, ref species);

        prevPosX = character.transform.position.x;
    }

    void Update()
    {
        if (!GameManager.Singleton.gameStarted || !GameManager.Singleton.gameRunning)
            return;

        if(mode == "test")
            test();
        else
            train();
    }

    private void test()
    {
        testNetwork.update(ref prevPosX, inputData);
        testNetwork.feedForward();
        testNetwork.applyOutput();
    }

    private void train()
    {
        double prevFitness = population[idPopulation].Fitness;
        bool clean = true;

        if (Input.GetKey(KeyCode.I))
        {
            ig.drawInfos(population, species, Globals.numberGeneration);
            clean = false;
        }

        if (Input.GetKey(KeyCode.N))
        {
            ig.drawNetwork(population[idPopulation]);
            clean = false;
        }

        if (clean)
            ig.clearGraphics();

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
            population = allPops.newGeneration(population, ref species);
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