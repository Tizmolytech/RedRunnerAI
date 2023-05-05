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

        for (ushort loop = 0; loop < numberNeurons; loop++)
        {
            neurons.Add(new Neuron(numberInputs));
        }
    }

    public List<Neuron> GetNeurons() { return neurons; }
    public int GetNumberNeurons() { return neurons.Count; }
}