using System.Collections.Generic;
using System;

public static class AllPops
{
    public static Network bestNetwork;
    public static Population newGeneration(in Population pop, ref List<Species> sortedSpecies)
    {
        Population newPop = new Population();
        newPop.Clear();

        int nbIndividualsToCreate = Globals.NB_INDIVIDUAL_POPULATION; // to keep track of how many individuals we have to create
        int indexNewSpecies = 0;


        double fitnessMaxOldPop = 0;
        Network oldBest = new Network(bestNetwork);
        bestNetwork = pop.getBestNetwork();
        double fitnessMaxPop = bestNetwork.Fitness;

        if (oldBest != null)
            fitnessMaxOldPop = oldBest.Fitness;

        if(fitnessMaxOldPop > fitnessMaxPop)
        {
            replaceBadPop(ref sortedSpecies, oldBest);
            bestNetwork = oldBest;
        }

        int nbTotalIndividual = 0;
        double globalAverageFitness = 0.0;
        
        foreach(Species s in sortedSpecies)
        {
            globalAverageFitness += s.computeFitnesses();
            nbTotalIndividual += s.Networks.Count;
        }

        globalAverageFitness /= nbTotalIndividual;

        sortedSpecies.Sort();

        foreach(Species s in sortedSpecies)
        {
            int nbIndividualSpecies = (int)Math.Ceiling((double)s.Networks.Count * s.AverageFitness / (double)globalAverageFitness);
            nbIndividualsToCreate -= nbIndividualSpecies;

            if(nbIndividualsToCreate < 0)
            {
                nbIndividualSpecies += nbIndividualsToCreate;
                nbIndividualsToCreate = 0;
            }

            s.NbChildren = nbIndividualSpecies;

            for(int i = 0; i < nbIndividualSpecies; i++)
            {
                if (indexNewSpecies > Globals.NB_INDIVIDUAL_POPULATION) break;

                Network network = NetworkToolbox.crossover(s.chooseParent(), s.chooseParent());

                if (bestNetwork.Fitness < Globals.FITNESS_GOAL) network.mutate();

                network.ParentSpecies = s;
                network.Fitness = 1;
                newPop.Add(new Network(network));
                indexNewSpecies += 1;
            }

            if (indexNewSpecies > Globals.NB_INDIVIDUAL_POPULATION) break;
        }

        for(int i = 0; i < sortedSpecies.Count; i++)
        {
            if (sortedSpecies[i].NbChildren > 0) continue;

            sortedSpecies.RemoveAt(i);
            i--;
        }

        Globals.numberGeneration++;

        bestNetwork.serialize();
        pop.serialize();

        return newPop;
    }

    private static void replaceBadPop(ref List<Species> sortedSpecies, Network oldBest)
    {
        foreach (Species s in sortedSpecies)
        {
            for (int i = 0; i < s.Networks.Count; i++)
            {
                s.Networks[i] = new Network(oldBest);
            }
        }
        Console.WriteLine("Bad population, we take the old best one");
        Console.WriteLine(oldBest);
    }

    //private Network? getOldBest()
    //{
    //    if (Count == 0) return null;

    //    double fitnessMaxOldPop = 0;
    //    Network? oldBest = null;

    //    foreach (Population p in this)
    //    {
    //        foreach (Network n in p)
    //        {
    //            if (fitnessMaxOldPop > n.Fitness) continue;

    //            fitnessMaxOldPop = n.Fitness;
    //            oldBest = n;
    //        }
    //    }

    //    return oldBest;
    //}
}