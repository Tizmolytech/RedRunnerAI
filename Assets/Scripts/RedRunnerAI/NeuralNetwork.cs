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
    private int score = 0;

    public NeuralNetwork(int numberInputs, int numberHiddenLayers, int numberNeuronsPerHiddenLayer)
    {
        this.numberInputs = numberInputs;
        this.numberHiddenLayers = numberHiddenLayers;
        this.numberNeuronsPerHiddenLayer = numberNeuronsPerHiddenLayer;
        layers = new List<Layer>();
        createNetwork();
    }

    private void createNetwork()
    {
        layers.Add(new Layer(numberNeuronsPerHiddenLayer, numberInputs));

        for (int i = 0; i < numberHiddenLayers - 1; i++)
        {
            layers.Add(new Layer(numberNeuronsPerHiddenLayer, numberNeuronsPerHiddenLayer));
        }

        layers.Add(new Layer(numberOutputs, numberNeuronsPerHiddenLayer));
    }

    private double sigmoid(double value)
    {
        return 1 / (1 + Math.Exp(-value));
    }

    public void forward(List<double> inputs)
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

            foreach (Neuron neuron in layer.getNeurons())
            {
                double sum = 0;
                neuron.setBias(inputsToLayer[inputsToLayer.Count - 1]);
                List<double> weights = neuron.getWeights();

                for (int i = 0; i < weights.Count; i++)
                {
                    sum += weights[i] * inputsToLayer[i];
                }

                neuron.setOutput(sigmoid(sum));
                outputs.Add(neuron.getOuput());
            }
        }
    }

    public void mutate()
    {
        Random random = new Random();

        foreach (Layer layer in layers)
        {
            List<Neuron> neurons = layer.getNeurons();
            foreach (Neuron neuron in neurons)
            {
                List<double> weights = neuron.getWeights();
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
                neuron.setWeights(weights);
            }
        }
    }

    public void displayFinalOuput()
    {
        foreach (Neuron neuron in getOutputLayer().getNeurons())
        {
            Console.WriteLine(neuron.getOuput());
        }
    }

    public List<Layer> getLayers() { return layers; }
    public int getNumberInputs() { return numberInputs; }
    public int getNumberOutputs() { return numberOutputs; }
    public int getNumberHiddenLayers() { return numberHiddenLayers; }
    public int getNumberNeuronsPerHiddenLayer() { return numberNeuronsPerHiddenLayer; }
    public Layer getOutputLayer() { return layers[layers.Count - 1]; }
    public int getScore() { return score; }
}