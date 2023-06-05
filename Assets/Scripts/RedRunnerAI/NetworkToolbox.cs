using System;

static class NetworkToolbox
{
    public static double getDiffWeight(in Network n1, in Network n2)
    {
        int numberConnections = 0;
        double total = 0.0;

        foreach(Connection c1 in n1.Connections)
        {
            foreach(Connection c2 in n2.Connections)
            {
                if (c1.Innovation != c2.Innovation) continue;

                numberConnections++;

                total += Math.Abs(c1.Weight - c2.Weight);
            }
        }

        //if no common connections, network are too different so we set the diff very high
        if(numberConnections == 0) 
        {
            return 100000; 
        }

        return total / numberConnections;
    }

    public static int getNumberDiffConnections(in Network n1, in Network n2)
    {
        int numberSame = 0;

        foreach(Connection c1 in n1.Connections)
            foreach (Connection c2 in n2.Connections)
                if (c1.Innovation == c2.Innovation)
                    numberSame++;

        return n1.Connections.Count + n2.Connections.Count - 2 * numberSame;
    }

    // get the score of a network to match with a species
    public static double getMatchingScore(in Network nTest, in Network nWit) 
    {
        return (Globals.EXCESS_COEF * getNumberDiffConnections(nWit, nTest)) / Math.Max(nTest.Connections.Count + nWit.Connections.Count, 1) + Globals.DIFF_WEIGHT_COEF * getDiffWeight(nWit, nTest);
    }

    public static Network crossover(in Network n1, in Network n2)
    {
        Network crossNetwork;

        Network goodNetwork;
        Network badNetwork;

        if(n1.Fitness > n2.Fitness)
        {
            badNetwork = n2;
            goodNetwork = n1;
        }
        else
        {
            badNetwork = n1;
            goodNetwork = n2;
        }

        crossNetwork = new Network(goodNetwork);

        for(int i = 0; i < crossNetwork.Connections.Count; i++)
        {
            foreach (Connection c2 in badNetwork.Connections)
            {
                if (crossNetwork.Connections[i].Innovation != c2.Innovation || new Random(Guid.NewGuid().GetHashCode()).NextDouble() <= 0.5) continue;
                
                crossNetwork.Connections[i] = new Connection(c2);
            }
        }

        crossNetwork.Fitness = 1;

        return crossNetwork;
    }
}