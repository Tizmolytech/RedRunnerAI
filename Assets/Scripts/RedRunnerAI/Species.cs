using System;
using System.Collections.Generic;

[Serializable]
public class Species : IComparable
{
    public double AverageFitness;
    public double MaxFitness;
    public List<Network> Networks { get; set; }

    public int NbChildren;

    public Species() 
    {
        Networks = new List<Network>();
        AverageFitness = 0;
        MaxFitness = 0;
        NbChildren = 0;
    }

    public Network chooseParent()
    {
        if (Networks.Count == 0)
            throw new Exception("Impossible to choose parent, there are no networks");

        if (Networks.Count == 1)
            return Networks[0];

        double totalFitness = 0.0;

        foreach (Network network in Networks)
            totalFitness += network.Fitness;

        int limit = new Random(Guid.NewGuid().GetHashCode()).Next((int)totalFitness);
        double total = 0;

        foreach(Network network in Networks)
        {
            total += network.Fitness;

            if(total >= limit)
                return new Network(network);
        }

        throw new Exception("Impossible to find a parent");
    }

    public int computeFitnesses()
    {
        int totalFitness = 0;
        AverageFitness = 0;
        MaxFitness = 0;

        foreach( Network network in Networks)
        {
            AverageFitness += network.Fitness;

            if (MaxFitness < network.Fitness)
                MaxFitness = network.Fitness;
        }
        totalFitness = (int)AverageFitness;
        AverageFitness /= Networks.Count;

        return totalFitness;
    }

    public int CompareTo(object? obj)
    {
        if(obj == null) return 1;

        Species s = (Species)obj;

        return s.MaxFitness.CompareTo(MaxFitness);
    }
}