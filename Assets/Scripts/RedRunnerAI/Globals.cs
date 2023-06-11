static class Globals
{
    public const int NB_OUTPUTS = 4;
    public const int GRID_H = 18;
    public const int GRID_W = 32;
    public const int MORE_INPUTS = 2;
    public const int NB_INPUTS = GRID_H * GRID_W + MORE_INPUTS; //change by number of values in grid
    public static int NB_INDIVIDUAL_POPULATION = 100; // number of individuals in a population when created
    public const double PROB_MUTATION_WEIGHT_RESET = 0.25; // probability to reset weight of a connection when mutating
    public const double MUTATION_WEIGHT_ADDED = 0.8; // weight added when mutating
    public const double PROB_MUTATION_WEIGHT = 0.95;
    public const double PROB_MUTATION_CONNECTION = 0.85;
    public const double PROB_MUTATION_NEURON = 0.39;
    public const double EXCESS_COEF = 0.50;
    public const double DIFF_WEIGHT_COEF = 0.92;
    public const double DIFF_LIMIT = 1.00;
    public const int NB_MAX_NEURONS = 100000;
    public const int FITNESS_GOAL = 100000;
    public static int numberInnovations = 0;
    public static int numberGeneration = 0;
    public const int NB_FRAME_RESET_BASE = 30;
    public const int NB_FRAME_RESET_PROGRESS = 100;
}