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

    public double GetBias() { return bias; }
    public double GetOuput() { return output; }
    public List<double> GetWeights() { return weights; }
    public void SetBias(double bias) { this.bias = bias; }
    public void SetWeights(List<double> newWeights) { weights = newWeights; }
    public void SetOutput(double output) { this.output = output; }
}