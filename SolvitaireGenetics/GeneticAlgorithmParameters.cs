using System.Text.Json;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithmParameters
{
    public abstract void SaveToFile(string filePath);

    public virtual string OutputDirectory { get; set; } = ".";
    public virtual int PopulationSize { get; set; } = 100;
    public virtual int Generations { get; set; } = 1000;
    public virtual double MutationRate { get; set; } = 0.01;
    public virtual int TournamentSize { get; set; } = 5;

    public static GeneticAlgorithmParameters LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);

        // Use a discriminator or heuristic to determine the parameter type
        if (json.Contains("\"DecksToUse\"")) // Example heuristic
        {
            return JsonSerializer.Deserialize<SolitaireGeneticAlgorithmParameters>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize SolitaireGeneticAlgorithmParameters.");
        }

        throw new NotSupportedException("Unknown parameter type in the configuration file.");
    }
}
