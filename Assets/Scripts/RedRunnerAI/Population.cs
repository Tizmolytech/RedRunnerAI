using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Population : List<Network>
{
    public Population()
    {
        for (int i = 0; i < Globals.NB_INDIVIDUAL_POPULATION; i++)
            Add(new Network());
    }

    public Population(in Network network)
    {
        for (int i = 0; i < Globals.NB_INDIVIDUAL_POPULATION; i++)
            Add(new Network(network));
    }

    public List<Species> sortPopulation()
    {
        List<Species> species = new List<Species>
        {
            new Species()
        };

        //in the first species we add the last network of the population
        species[0].Networks.Add(this[Count - 1]);

        for (int i = 0; i < Count - 1; i++)
        {
            bool found = false;

            foreach(Species s in species)
            {
                Network wit = s.Networks[new System.Random(Guid.NewGuid().GetHashCode()).Next(s.Networks.Count)];

                if (NetworkToolbox.getMatchingScore(this[i], wit) >= Globals.DIFF_LIMIT) continue;

                s.Networks.Add(this[i]);
                found = true;
                break;
            }

            if (found) continue;

            species.Add(new Species());
            species[species.Count - 1].Networks.Add(this[i]);
        }

        return species;
    }

    public Network getBestNetwork()
    {
        double fitnessMaxPop = 0.0;
        Network bestNetwork = this[0];

        foreach (Network network in this)
        {
            if (fitnessMaxPop >= network.Fitness) continue;

            fitnessMaxPop = network.Fitness;

            bestNetwork = network;
        }

        return bestNetwork;
    }

    public void serialize()
    {
        string fName = "Pops\\population_" + Globals.numberGeneration.ToString() + ".json"; 
        string json = JsonUtility.ToJson(this);

        File.WriteAllText(fName, json);
    }

    public void deserialize(int idGeneration)
    {
        string fName = "Pops\\population_" + idGeneration.ToString() + ".json";

        string json = File.ReadAllText(fName);

        Population? pop = JsonUtility.FromJson<Population>(json);

        if (pop == null) return;

        Clear();

        foreach(Network n in pop)
            Add(n);
    }
}