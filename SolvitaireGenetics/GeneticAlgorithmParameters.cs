using System.Text.Json;
using SolvitaireCore;
using SolvitaireGenetics.IO;
using SolvitaireIO;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithmParameters
{
    public virtual string? OutputDirectory { get; set; } = null;
    public virtual int PopulationSize { get; set; } = 1000;
    public virtual int Generations { get; set; } = 100;
    public virtual double MutationRate { get; set; } = 0.01;
    public virtual int TournamentSize { get; set; } = 5;
    public virtual double TemplateInitialRatio { get; set; } = .1;
    public virtual LoggingType LoggingType { get; set; } = LoggingType.Json;
    public Chromosome? TemplateChromosome { get; set; } = null;


    public static GeneticAlgorithmParameters LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);

        // Use a discriminator or heuristic to determine the parameter type
        if (json.Contains("\"DecksToUse\""))
        {
            return JsonSerializer.Deserialize<SolitaireGeneticAlgorithmParameters>(json,
                       new JsonSerializerOptions() { Converters = { new ChromosomeConverter<SolitaireChromosome>() } })
                   ?? throw new InvalidOperationException("Failed to deserialize SolitaireGeneticAlgorithmParameters.");
        }
        if (json.Contains("\"CorrectA\"")) 
        {
            return JsonSerializer.Deserialize<QuadraticGeneticAlgorithmParameters>(json,
            new JsonSerializerOptions() { Converters = { new ChromosomeConverter<QuadraticChromosome>() } })
                   ?? throw new InvalidOperationException("Failed to deserialize SolitaireGeneticAlgorithmParameters.");
        }

        throw new NotSupportedException("Unknown parameter type in the configuration file.");
    }

    public virtual void SaveToFile(string filePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}
