using System;

[Serializable]
public class Connection
{
    public int Input;
    public int Output;
    public bool Active;
    public double Weight;
    public int Innovation;
    public bool On; //just for drawing

    public Connection(int input, int output)
    {
        Input = input;
        Output = output;
        Active = true;
        Weight = Utils.generateWeight();
        Innovation = Globals.numberInnovations;
        Globals.numberInnovations += 1;
        On = false;
    }

    public Connection(in Connection connection)
    {
        Input = connection.Input;
        Output = connection.Output;
        Active = connection.Active;
        Weight = connection.Weight;
        Innovation = connection.Innovation;
        On = connection.On;
    }
}