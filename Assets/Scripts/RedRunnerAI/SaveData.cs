using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

#pragma warning disable SYSLIB0011
/**
* Class to save and load the neural network model
**/
public static class SaveData
{
    public static void saveToFile(string fileName, NeuralNetwork neuralNetwork)
    {
        FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);

        IFormatter formatter = new BinaryFormatter();
        formatter.Serialize(fs, neuralNetwork);
        fs.Close();
    }

    public static NeuralNetwork loadFromFile(string fileName)
    {
        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

        IFormatter formatter = new BinaryFormatter();
        NeuralNetwork neuralNetwork = (NeuralNetwork)formatter.Deserialize(fs);
        fs.Close();

        return neuralNetwork;
    }
}