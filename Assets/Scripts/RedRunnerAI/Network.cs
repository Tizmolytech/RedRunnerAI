using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using RedRunner.Characters;

[Serializable]
public class Network
{
    public double Fitness;
    public Species ParentSpecies;
    public List<Neuron> Neurons;
    public List<Connection> Connections;

    public Network()
    {
        Fitness = 1;
        Neurons = new List<Neuron>();
        Connections = new List<Connection>();
        ParentSpecies = new Species();

        int i = 1;

        for (i = 1; i <= Globals.NB_INPUTS; i++)
        {
            addNeuron(new Neuron(i, 1, "input"));
        }

        for (i = Globals.NB_INPUTS + 1; i <= Globals.NB_OUTPUTS + Globals.NB_INPUTS; i++)
        {
            addNeuron(new Neuron(i, 0, "output"));
        }
    }

    public Network(in Network network)
    {
        Fitness = network.Fitness;
        Neurons = new List<Neuron>(network.Neurons.Count);

        foreach (Neuron neuron in network.Neurons)
            Neurons.Add(new Neuron(neuron));

        Connections = new List<Connection>(network.Connections.Count);

        foreach (Connection connection in network.Connections)
            Connections.Add(new Connection(connection));

        ParentSpecies = network.ParentSpecies;
    }

    private void addNeuron(in Neuron neuron)
    {
        Neurons.Add(neuron);
    }

    private void connect(int input, int output)
    {
        Connections.Add(new Connection(input, output));
    }

    private void mutateWeightsConnections()
    {
        foreach(Connection connection in Connections)
        {
            if (!connection.Active)
                continue;

            if(new System.Random(Guid.NewGuid().GetHashCode()).NextDouble() < Globals.PROB_MUTATION_WEIGHT_RESET)
            {
                connection.Weight = Utils.generateWeight();
            }
            else
            {
                if (new System.Random(Guid.NewGuid().GetHashCode()).NextDouble() >= 0.5)
                    connection.Weight -= Globals.MUTATION_WEIGHT_ADDED;
                else
                    connection.Weight += Globals.MUTATION_WEIGHT_ADDED;
            }
        }
    }

    private void mutateAddConnection()
    {
        System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
        List<Neuron> shuffledNeurons = Neurons.OrderBy(x => random.Next()).ToList();
        bool finished = false;

        foreach(Neuron nIn in shuffledNeurons)
        {
            foreach (Neuron nOut in shuffledNeurons)
            {
                if(nIn.Id == nOut.Id) continue;

                if (nIn.Type.Equals("output") || nOut.Type.Equals("input"))//maybe add input/hidden
                    continue;

                bool alreadyConnected = false;

                foreach(Connection con in Connections)
                {
                    if(con.Input == nIn.Id && con.Output == nOut.Id)
                    {
                        alreadyConnected = true;
                        break;
                    }
                }

                if(!alreadyConnected)
                {
                    finished = true;
                    connect(nIn.Id, nOut.Id);
                    break;
                }
            }

            if (finished) break;
        }

        if (!finished)
            Console.WriteLine("Impossible to create connection");
    }

    private void mutateAddNeuron() //adds an hidden neuron between 2 connected neurons
    {
        if (Connections.Count == 0)
        {
            Console.WriteLine("Impossible to add neuron: no connections");
            return;
        }

        if (Neurons.Count == Globals.NB_MAX_NEURONS)
        {
            Console.WriteLine("Impossible to add neuron: too many neurons");
            return;
        }

        System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
        List<Connection> shuffledConnections = Connections.OrderBy(x => random.Next()).ToList();

        foreach(Connection con in shuffledConnections)
        {
            if(!con.Active) continue;

            con.Active = false;
            Neuron newNeuron = new Neuron((int)Neurons.Count + 1, 1, "hidden");
            addNeuron(newNeuron);

            connect(con.Input, newNeuron.Id);
            connect(newNeuron.Id, con.Output);

            break;
        }
    }

    public void mutate()
    {
        double random = new System.Random(Guid.NewGuid().GetHashCode()).NextDouble();

        if(random < Globals.PROB_MUTATION_WEIGHT)
            mutateWeightsConnections();
        if(random < Globals.PROB_MUTATION_NEURON)
            mutateAddNeuron();
        if(random < Globals.PROB_MUTATION_CONNECTION)
            mutateAddConnection();
    }

    //compute new values for neurons
    public void feedForward()
    {
        foreach(Connection con in Connections)
        {
            if(con.Active)
            {
                Neurons[con.Output - 1].Value = 0;
                con.On = false;
            }
        }

        foreach(Connection con in Connections)
        {
            if(!con.Active) continue;

            double beforeProcess = Neurons[con.Output - 1].Value;
            Neurons[con.Output - 1].Value = Neurons[con.Input - 1].Value * con.Weight + Neurons[con.Output - 1].Value;

            //if the value change we put on the connection (to draw)
            if(beforeProcess != Neurons[con.Output - 1].Value)
                con.On = true;
            else
                con.On = false;
        }
    }

    public void update(ref double prevPosX, InputData id)
    {
        double posX = GameObject.Find("RedRunner").GetComponent<Character>().transform.position.x;
        if (posX > prevPosX) //increase fitness if the character goes to the right
        {
            Fitness = posX;
            prevPosX = posX;
        }
        int[] input = id.GetDatasOneLine(); //replace by the grid map

        //we feed the network with the new input
        for(int i = 0; i < Globals.NB_INPUTS; i++)
            Neurons[i].Value = input[i];
    }

    public void serialize()
    {
        string fName = "Networks\\best_" + Globals.numberGeneration.ToString() + ".json";
        string json = JsonUtility.ToJson(this);
      
        File.WriteAllText(fName, json);
    }

    public void deserialize(int idGeneration)
    {
        string fName = "Networks\\best_" + idGeneration.ToString() + ".json";

        string json = File.ReadAllText(fName);

        Network? network = JsonUtility.FromJson<Network>(json);

        if (network == null) return;

        Fitness = network.Fitness;
        ParentSpecies = network.ParentSpecies;
        Neurons = network.Neurons;
        Connections = network.Connections;
    }

    public void applyOutput()
    {
        Character player = GameObject.Find("RedRunner").GetComponent<Character>();
        bool[] zqsd = new bool[Globals.NB_OUTPUTS];

        for (int i = 0; i < Globals.NB_OUTPUTS; i++)
            zqsd[i] = Utils.sigmoid(Neurons[i + Globals.NB_INPUTS].Value);

        if (zqsd[1] && zqsd[3]) //right take over left
            zqsd[1] = false;

        if (zqsd[1]) //q
            player.Move(-1f);
        else if (zqsd[3]) //d
            player.Move(1f);
        else
            player.Move(0f);
        if (zqsd[0]) //z
            player.Jump();
        if (zqsd[2]) //s
            player.Roll();
    }
}