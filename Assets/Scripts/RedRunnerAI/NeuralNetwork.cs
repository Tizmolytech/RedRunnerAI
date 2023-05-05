using System;
using System.Collections.Generic;

/**
 * Class that represents our neural network
 * Here, we have 4 outputs:
 * 1st output - jump
 * 2nd output - right
 * 3rd output - left
 * 4th ouput - down
**/
[Serializable]
public class NeuralNetwork
{
    private List<Layer> layers;
    private int numberInputs;
    private const int numberOutputs = 4;
    private int numberHiddenLayers;
    private int numberNeuronsPerHiddenLayer;
    private ushort score = 0;

    public NeuralNetwork(int numberInputs, int numberHiddenLayers, int numberNeuronsPerHiddenLayer)
    {
        this.numberInputs = numberInputs;
        this.numberHiddenLayers = numberHiddenLayers;
        this.numberNeuronsPerHiddenLayer = numberNeuronsPerHiddenLayer;
        layers = new List<Layer>();
        CreateNetwork();
    }

    private void CreateNetwork()
    {
        layers.Add(new Layer(numberNeuronsPerHiddenLayer, numberInputs));

        for (int loop = 0; loop < numberHiddenLayers - 1; loop++)
        {
            layers.Add(new Layer(numberNeuronsPerHiddenLayer, numberNeuronsPerHiddenLayer));
        }

        layers.Add(new Layer(numberOutputs, numberNeuronsPerHiddenLayer));
    }

    private double Sigmoid(double value)
    {
        return 1 / (1 + Math.Exp(-value));
    }

    public void Forward(List<double> inputs)
    {
        if (inputs.Count != numberInputs)
        {
            Console.WriteLine("Error: the number of inputs is not correct");
            return;
        }

        List<double> outputs = inputs;

        foreach (Layer layer in layers)
        {
            List<double> inputsToLayer = outputs;
            outputs = new List<double>();

            foreach (Neuron neuron in layer.GetNeurons())
            {
                double sum = 0;
                neuron.SetBias(inputsToLayer[inputsToLayer.Count - 1]);
                List<double> weights = neuron.GetWeights();

                for (int i = 0; i < weights.Count; i++)
                {
                    sum += weights[i] * inputsToLayer[i];
                }

                neuron.SetOutput(Sigmoid(sum));
                outputs.Add(neuron.GetOuput());
            }
        }
    }

    public void Mutate()
    {
        Random random = new Random();

        foreach (Layer layer in layers)
        {
            List<Neuron> neurons = layer.GetNeurons();
            foreach (Neuron neuron in neurons)
            {
                List<double> weights = neuron.GetWeights();
                for (int i = 0; i < weights.Count; i++)
                {
                    double weight = weights[i];
                    double randomNumber = random.NextDouble() * 10f;
                    if (randomNumber <= 2f)
                    {
                        weight *= -1f;
                    }
                    else if (randomNumber <= 4f)
                    {
                        weight = random.NextDouble() * 10f;
                    }
                    else if (randomNumber <= 6f)
                    {
                        double factor = random.NextDouble() * 10f + 1f;
                        weight *= (double)factor;
                    }
                    else if (randomNumber <= 8f)
                    {
                        double factor = random.NextDouble() * 10f;
                        weight *= (double)factor;
                    }
                    weights[i] = weight;
                }
                neuron.SetWeights(weights);
            }
        }
    }

    public void DisplayFinalOuput()
    {
        foreach (Neuron neuron in GetOutputLayer().GetNeurons())
        {
            Console.WriteLine(neuron.GetOuput());
        }
    }

    public List<Layer> GetLayers() { return layers; }
    public int GetNumberInputs() { return numberInputs; }
    public int GetNumberOutputs() { return numberOutputs; }
    public int GetNumberHiddenLayers() { return numberHiddenLayers; }
    public int GetNumberNeuronsPerHiddenLayer() { return numberNeuronsPerHiddenLayer; }
    public Layer GetOutputLayer() { return layers[layers.Count - 1]; }
    public ushort GetScore() { return score; }
}