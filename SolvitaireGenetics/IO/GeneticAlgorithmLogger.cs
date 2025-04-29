using System.Text.Json;
using SolvitaireIO;

namespace SolvitaireGenetics;

/// <summary>  
/// Logs Generational and Agent specific data throughout a genetic algorithm run.   
/// </summary>  
/// <typeparam name="TChromosome"></typeparam>  
public class GeneticAlgorithmLogger<TChromosome> where TChromosome : Chromosome<TChromosome>
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _generationLogFilePath;
    private readonly string _agentLogFilePath;
    private readonly object _generationLogLock = new();
    private readonly object _agentLogLock = new();
    private readonly List<AgentLog> _agentLogBatch = new(); // In-memory batch for AgentLogs

    public GeneticAlgorithmLogger(string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        _generationLogFilePath = Path.Combine(outputDirectory, "GenerationalLog.json").GetUniqueFilePath();
        _agentLogFilePath = Path.Combine(outputDirectory, "AgentLog.json").GetUniqueFilePath();

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Makes the JSON output more readable  
        };

        // Ensure the files exist  
        lock (_generationLogLock)
        {
            if (!File.Exists(_generationLogFilePath))
            {
                File.WriteAllText(_generationLogFilePath, "[]"); // Initialize with an empty JSON array  
            }
        }

        lock (_agentLogLock)
        {
            if (!File.Exists(_agentLogFilePath))
            {
                File.WriteAllText(_agentLogFilePath, "[]"); // Initialize with an empty JSON array  
            }
        }
    }

    /// <summary>  
    /// Logs generational information, including best and average fitness and chromosomes.  
    /// </summary>  
    public void LogGenerationInfo(int generation, double bestFitness, double averageFitness, TChromosome bestChromosome, TChromosome averageChromosome, TChromosome stdChromosome)
    {
        var generationLog = new GenerationLogDto
        {
            Generation = generation,
            BestFitness = bestFitness,
            AverageFitness = averageFitness,
            BestChromosome = new ChromosomeDto { Weights = bestChromosome.MutableStatsByName },
            AverageChromosome = new ChromosomeDto { Weights = averageChromosome.MutableStatsByName },
            StdChromosome = new ChromosomeDto { Weights = stdChromosome.MutableStatsByName }
        };

        lock (_generationLogLock)
        {
            // Read existing generations from the file  
            var existingGenerations = JsonSerializer.Deserialize<List<GenerationLogDto>>(File.ReadAllText(_generationLogFilePath), _jsonOptions) ?? new List<GenerationLogDto>();

            // Add the new generation log  
            existingGenerations.Add(generationLog);

            // Write back to the file  
            File.WriteAllText(_generationLogFilePath, JsonSerializer.Serialize(existingGenerations, _jsonOptions));
        }

        // Log to console  
        Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Generation Log: {JsonSerializer.Serialize(generationLog, _jsonOptions)}");
    }

    /// <summary>  
    /// Logs detailed information for a single agent in the generation.  
    /// </summary>  
    public void LogAgentDetail(int generation, TChromosome chromosome, double fitness, int gamesWon, int movesMade, int gamesPlayed)
    {
        var agentLog = new AgentLog
        {
            Generation = generation,
            Fitness = fitness,
            GamesWon = gamesWon,
            MovesMade = movesMade,
            GamesPlayed = gamesPlayed,
            Chromosome = new ChromosomeDto { Weights = chromosome.MutableStatsByName }
        };

        lock (_agentLogLock)
        {
            // Read existing agent logs from the file  
            var existingAgentLogs = JsonSerializer.Deserialize<List<AgentLog>>(File.ReadAllText(_agentLogFilePath), _jsonOptions) ?? new List<AgentLog>();

            // Add the new agent log  
            existingAgentLogs.Add(agentLog);

            // Write back to the file  
            File.WriteAllText(_agentLogFilePath, JsonSerializer.Serialize(existingAgentLogs, _jsonOptions));
        }
    }

    /// <summary>
    /// Accumulates detailed information for a single agent in the generation.
    /// </summary>
    public void AccumulateAgentLog(int generation, TChromosome chromosome, double fitness, int gamesWon, int movesMade, int gamesPlayed)
    {
        var agentLog = new AgentLog
        {
            Generation = generation,
            Fitness = fitness,
            GamesWon = gamesWon,
            MovesMade = movesMade,
            GamesPlayed = gamesPlayed,
            Chromosome = new ChromosomeDto { Weights = chromosome.MutableStatsByName }
        };

        lock (_agentLogLock)
        {
            _agentLogBatch.Add(agentLog);
        }
    }

    /// <summary>
    /// Writes out the accumulated AgentLogs to the file and clears the in-memory batch.
    /// </summary>
    public void FlushAgentLogs()
    {
        lock (_agentLogLock)
        {
            // Read existing agent logs from the file
            var existingAgentLogs = JsonSerializer.Deserialize<List<AgentLog>>(File.ReadAllText(_agentLogFilePath), _jsonOptions) ?? new List<AgentLog>();

            // Add the accumulated logs
            existingAgentLogs.AddRange(_agentLogBatch);

            // Write back to the file
            File.WriteAllText(_agentLogFilePath, JsonSerializer.Serialize(existingAgentLogs, _jsonOptions));

            // Clear the in-memory batch
            _agentLogBatch.Clear();
        }
    }
}
