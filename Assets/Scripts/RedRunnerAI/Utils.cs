using System;

static class Utils
{
    public static double generateWeight()
    {
        double weight = 1;
        if (new Random(Guid.NewGuid().GetHashCode()).NextDouble() >= 0.5)
            weight *= -1;

        return weight;
    }

    public static bool sigmoid(double x)
    {
        double res = x / (1.0 + Math.Abs(x));

        if (res >= 0.5)
            return true;

        return false;
    }

    public static int from2Dto1D(int x, int y)
    {
        return x + y * Globals.GRID_W;
    }
}