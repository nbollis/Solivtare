using CommandLine;
using System.Text.Json;
using SolvitaireCore.Wordle;
using SolvitaireGenetics.IO;
using SolvitaireIO;

namespace SolvitaireGenetics;

public class WordleGeneticAlgorithmParameters : GeneticAlgorithmParameters
{
    public WordleGeneticAlgorithmParameters() : base()
    {
        TemplateChromosome = WordleChromosome.BestSoFar();
        
        // Default first word pool - research-backed optimal openers
        FirstWordPool = new List<string>
        {
            "OCEAN", "SPEAR", "AUDIO"
        };
    }

    [Option('o', "output", Required = true, HelpText = "Directory for output files.")]
    public override string? OutputDirectory { get; set; } = null;

    [Option('p', "population", Default = 100, HelpText = "Size of the population.")]
    public override int PopulationSize { get; set; } = 100;

    [Option('g', "generations", Default = 100, HelpText = "Number of generations.")]
    public override int Generations { get; set; } = 100;

    [Option('m', "mutation", Default = 0.01, HelpText = "Mutation rate.")]
    public override double MutationRate { get; set; } = 0.01;

    [Option('t', "tournament", Default = 5, HelpText = "Tournament size for selection.")]
    public override int TournamentSize { get; set; } = 5;

    [Option('s', "template", Default = 0.1, HelpText = "Initial ratio of template chromosomes.")]
    public override double TemplateInitialRatio { get; set; } = .1;

    [Option('z', "logging", Default = LoggingType.Json, HelpText = "Logging type (Json or Database).")]
    public override LoggingType LoggingType { get; set; } = LoggingType.Json;

    [Option('w', "wordpool", Required = false, HelpText = "Path to a text file containing first word pool (one word per line).")]
    public string? FirstWordPoolFile { get; set; } = null;

    [Option('n', "games", Default = 100, HelpText = "Number of games to play per agent for fitness evaluation.")]
    public int GamesPerAgent { get; set; } = 100;

    [Option('x', "maxguesses", Default = 6, HelpText = "Maximum number of guesses allowed per game.")]
    public int MaxGuesses { get; set; } = 6;

    [Option('l', "wordlength", Default = 5, HelpText = "Length of words (default 5 for standard Wordle).")]
    public int WordLength { get; set; } = 5;

    /// <summary>
    /// Pool of candidate first words for the genetic algorithm to choose from.
    /// The chromosome's FirstWordIndex will select from this pool.
    /// </summary>
    public List<string> FirstWordPool { get; set; }

    /// <summary>
    /// Bonus points per guess saved (earlier wins score higher)
    /// </summary>
    [Option('b', "speedbonus", Default = 1.0, HelpText = "Bonus points per guess saved when winning.")]
    public double SpeedBonus { get; set; } = 1.0;

    /// <summary>
    /// Penalty for losing a game
    /// </summary>
    [Option('e', "losspenalty", Default = -5.0, HelpText = "Penalty for losing a game.")]
    public double LossPenalty { get; set; } = -5.0;

    /// <summary>
    /// Use a fixed set of target words for consistent evaluation
    /// </summary>
    public bool UseFixedTargetWords { get; set; } = true;

    /// <summary>
    /// Fixed target words for evaluation (loaded or generated)
    /// </summary>
    public List<string>? FixedTargetWords { get; set; } = null;

    public override void SaveToFile(string filePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Converters = { new ChromosomeConverter<WordleChromosome>() }
        });
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Load first word pool from file if specified
    /// </summary>
    public void LoadFirstWordPool()
    {
        if (!string.IsNullOrEmpty(FirstWordPoolFile) && File.Exists(FirstWordPoolFile))
        {
            FirstWordPool = File.ReadAllLines(FirstWordPoolFile)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim().ToUpperInvariant())
                .Where(word => word.Length == WordLength && WordleWordList.IsValidAnswer(word))
                .ToList();

            if (FirstWordPool.Count == 0)
            {
                throw new InvalidOperationException($"No valid words found in {FirstWordPoolFile}");
            }
        }
    }

    /// <summary>
    /// Generate or load fixed target words for consistent evaluation
    /// </summary>
    public void PrepareTargetWords()
    {
        if (UseFixedTargetWords && FixedTargetWords == null)
        {
            // Use a fixed set of common Wordle answers for consistency
            var allAnswers = WordleWordList.AnswerWords.ToList();
            var random = new Random(42); // Fixed seed for reproducibility
            
            // Select a subset for evaluation (more than we need so we can cycle through them)
            FixedTargetWords = allAnswers
                .OrderBy(_ => random.Next())
                .Take(Math.Max(GamesPerAgent * 2, 500))
                .ToList();
        }
    }
}
