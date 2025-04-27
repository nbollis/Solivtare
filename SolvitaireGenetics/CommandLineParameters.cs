using CommandLine;

namespace SolvitaireGenetics;

public class CommandLineParameters
{
    [Option('i', "input", Required = false, HelpText = "Path to the input file.")]
    public string? InputFile { get; set; } // TODO: add optional input file to allow algorithm to load a previous state

    [Option('o', "output", Required = true, HelpText = "Directory for output files.")]
    public string? OutputDirectory { get; set; }

    [Option('p', "population", Default = 100, HelpText = "Size of the population.")]
    public int PopulationSize { get; set; }

    [Option('g', "generations", Default = 1000, HelpText = "Number of generations.")]
    public int Generations { get; set; }

    [Option('m', "mutation", Default = 0.01, HelpText = "Mutation rate.")]
    public double MutationRate { get; set; }

    [Option('t', "tournament", Default = 5, HelpText = "Tournament size for selection.")]
    public int TournamentSize { get; set; }

    [Option('t', "maxmoves", Default = 1000, HelpText = "Maximum number of moves per game.")]
    public int MaxMovesPerGeneration { get; set; }

    [Option('l', "limit", Default = 100, HelpText = "Maximum number of games per generation.")]
    public int MaxGamesPerGeneration { get; set; }

    //[Option('c', "crossover", Default = 0.5, HelpText = "Crossover rate.")]
    //public double CrossoverRate { get; set; }

    //[Option('a', "agent", Default = "MaxiMaxAgent", HelpText = "Name of the agent to use.")]
    //public string? AgentName { get; set; }
}