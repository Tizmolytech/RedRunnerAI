using System;
using System.Collections.Generic;

/**
 * Class that represents a layer
**/
[Serializable]
public class Layer
{
    private List<Neuron> neurons;

    public Layer(int numberNeurons, int numberInputs)
    {
        neurons = new List<Neuron>();

        for (int i = 0; i < numberNeurons; i++)
        {
            neurons.Add(new Neuron(numberInputs));
        }
    }

    public List<Neuron> getNeurons() { return neurons; }
    public int getNumberNeurons() { return neurons.Count; }
}