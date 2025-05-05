using CommandLine;
using System.Text.Json;

namespace SolvitaireGenetics;

public class SolitaireGeneticAlgorithmParameters : GeneticAlgorithmParameters
{
    // TODO: add optional input file to allow algorithm to load a previous state

    public SolitaireGeneticAlgorithmParameters() : base()
    {
        TemplateChromosome = GeneticSolitaireAlgorithm.BestSoFar();
    }


    [Option('o', "output", Required = true, HelpText = "Directory for output files.")]
    public override string? OutputDirectory { get; set; } = null;

    [Option('p', "population", Default = 100, HelpText = "Size of the population.")]
    public override int PopulationSize { get; set; } = 100;

    [Option('g', "generations", Default = 1000, HelpText = "Number of generations.")]
    public override int Generations { get; set; } = 1000;

    [Option('m', "mutation", Default = 0.01, HelpText = "Mutation rate.")]
    public override double MutationRate { get; set; } = 0.01;

    [Option('t', "tournament", Default = 5, HelpText = "Tournament size for selection.")]
    public override int TournamentSize { get; set; } = 5;


    [Option('d', "deckjson", Required = false, HelpText = "Path to a json file of serialized decks to use.")]
    public string? DecksToUse { get; set; } = null;

    [Option('c', "maxmoves", Default = 1000, HelpText = "Maximum number of moves per game.")]
    public int MaxMovesPerGeneration { get; set; } = 1000;

    [Option('l', "limit", Default = 10, HelpText = "Maximum number of games per generation.")]
    public int MaxGamesPerGeneration { get; set; } = 10;

    public override void SaveToFile(string filePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}