using System.Collections.Generic;
using System;

/**
 * Class that represents a neuron
**/

[Serializable]
public class Neuron
{
    private double bias;
    private List<double> weights;
    private double output;

    public Neuron(int numberInputs)
    {
        Random random = new Random();
        weights = new List<double>();
        bias = random.NextDouble() * 2 - 1;

        for (int i = 0; i < numberInputs; i++)
        {
            weights.Add(random.NextDouble() * 2 - 1);
        }
    }

    public double getBias() { return bias; }
    public double getOuput() { return output; }
    public List<double> getWeights() { return weights; }
    public void setBias(double bias) { this.bias = bias; }
    public void setWeights(List<double> newWeights) { weights = newWeights; }
    public void setOutput(double output) { this.output = output; }
}