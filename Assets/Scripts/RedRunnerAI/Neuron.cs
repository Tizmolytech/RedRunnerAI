using System;
using System.Collections.Generic;

[Serializable]
public class Neuron
{
    public int Id;
    public double Value;
    public string Type;

    public Neuron()
    {
        Id = 0;
        Value = 0.0;
        Type = "";
    }

    public Neuron(int id, double value, string type)
    {
        Id = id;
        Value = value;
        Type = type;
    }

    public Neuron(in Neuron neuron)
    {
        Id = neuron.Id;
        Value = neuron.Value;
        Type = neuron.Type;
    }
}


