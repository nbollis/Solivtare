using CommandLine;
using System.Text.Json;

namespace SolvitaireGenetics;

public class SolitaireGeneticAlgorithmParameters
{
    // TODO: add optional input file to allow algorithm to load a previous state
    
    [Option('o', "output", Required = true, HelpText = "Directory for output files.")]
    public string? OutputDirectory { get; set; }

    [Option('d', "deckjson", Required = false, HelpText = "Path to a json file of serialized decks to use.")]
    public string? DecksToUse { get; set; }

    [Option('p', "population", Default = 100, HelpText = "Size of the population.")]
    public int PopulationSize { get; set; }

    [Option('g', "generations", Default = 1000, HelpText = "Number of generations.")]
    public int Generations { get; set; }

    [Option('m', "mutation", Default = 0.01, HelpText = "Mutation rate.")]
    public double MutationRate { get; set; }

    [Option('t', "tournament", Default = 5, HelpText = "Tournament size for selection.")]
    public int TournamentSize { get; set; }

    [Option('c', "maxmoves", Default = 1000, HelpText = "Maximum number of moves per game.")]
    public int MaxMovesPerGeneration { get; set; }

    [Option('l', "limit", Default = 100, HelpText = "Maximum number of games per generation.")]
    public int MaxGamesPerGeneration { get; set; }

    public static SolitaireGeneticAlgorithmParameters LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<SolitaireGeneticAlgorithmParameters>(json) ?? throw new InvalidOperationException("Failed to deserialize configuration.");
    }

    public void SaveToFile(string filePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}