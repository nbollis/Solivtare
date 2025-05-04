using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using SolvitaireIO;

namespace SolvitaireGenetics;

/// <summary>  
/// Logs Generational and Agent specific data throughout a genetic algorithm run.   
/// </summary>  
/// <typeparam name="TChromosome"></typeparam>  
public class GeneticAlgorithmLogger<TChromosome> where TChromosome : Chromosome, new()
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string? _generationLogFilePath;
    private readonly string? _agentLogFilePath;
    private readonly object _generationLogLock = new();
    private readonly object _agentLogLock = new();
    private readonly List<GenerationLogDto> _generationLog = new();
    private int _lastGenerationNumber;
    private readonly ConcurrentBag<AgentLog> _agentLogBatch = new(); // In-memory batch for AgentLogs
    internal readonly string OutputDirectory;
    public int BatchCount => _agentLogBatch.Count;

    public GeneticAlgorithmLogger(string? outputDirectory, bool ensureUniquePath = false)
    {
        if (string.IsNullOrEmpty(outputDirectory))
            return;
        OutputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory), "Output directory cannot be null.");
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        _generationLogFilePath = Path.Combine(outputDirectory, "GenerationalLog.json");
        _agentLogFilePath = Path.Combine(outputDirectory, "AgentLog.json");
        if (ensureUniquePath)
        {
            _generationLogFilePath = _generationLogFilePath.GetUniqueFilePath();
            _agentLogFilePath = _agentLogFilePath.GetUniqueFilePath();
        }

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true, // Makes the JSON output more readable  
            Converters = { new ChromosomeConverter<TChromosome>() }
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

    public void SubscribeToAlgorithm(IGeneticAlgorithm algorithm)
    {
        algorithm.GenerationCompleted += (generation, generationLog) =>
        {
            LogGenerationInfo(generationLog);
        };

        algorithm.AgentCompleted += AccumulateAgentLog;
    }

    #region Log Directly to File/Memory

    /// <summary>  
    /// Logs generational information, including best and average fitness and chromosomes.  
    /// </summary>  
    public void LogGenerationInfo(int generation, double bestFitness, double averageFitness, double stdFitness, TChromosome bestChromosome, TChromosome averageChromosome, TChromosome stdChromosome)
    {
        var generationLog = new GenerationLogDto
        {
            Generation = generation,
            BestFitness = bestFitness,
            AverageFitness = averageFitness,
            StdFitness = stdFitness,
            BestChromosome = bestChromosome,
            AverageChromosome = averageChromosome,
            StdChromosome = stdChromosome
        };

        LogGenerationInfo(generationLog);
    }

    public void LogGenerationInfo(GenerationLogDto generationLog)
    {
        _lastGenerationNumber = generationLog.Generation;

        if (_generationLogFilePath is null)
            return;

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
        Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Generation: {generationLog.Generation}. Best Fitness: {generationLog.BestFitness}. Average Fitness: {generationLog.AverageFitness}");
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
            Chromosome = chromosome
        };

        LogAgentDetail(agentLog);
    }

    /// <summary>  
    /// Logs detailed information for a single agent in the generation.  
    /// </summary>  
    public void LogAgentDetail(AgentLog agentLog)
    {
        if (_agentLogFilePath is null)
            return;

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


    #endregion

    #region Accumulate Logs Until Flush

    /// <summary>
    /// Accumulates detailed information for a single agent in the generation. Write the batch with FlushAgentLogs.
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
            Chromosome = chromosome
        };

        lock (_agentLogLock)
        {
            _agentLogBatch.Add(agentLog);
        }
    }

    /// <summary>
    /// Accumulates detailed information for a single agent in the generation. Write the batch with FlushAgentLogs.
    /// </summary>
    public void AccumulateAgentLog(AgentLog agentLog)
    {
        _agentLogBatch.Add(agentLog);
    }

    /// <summary>
    /// Writes out the accumulated AgentLogs to the file and clears the in-memory batch.
    /// </summary>
    public void FlushAgentLogs(int currentGeneration, List<TChromosome> population)
    {
        if (_agentLogFilePath is not null)
        {
            lock (_agentLogLock)
            {
                // Read existing agent logs from the file
                var existingAgentLogs =
                    JsonSerializer.Deserialize<List<AgentLog>>(File.ReadAllText(_agentLogFilePath), _jsonOptions) ??
                    new List<AgentLog>();

                // reverse for easier lookup
                existingAgentLogs.Reverse();

                // Populate count information and ensure all chromosomes are represented in the log, even if they were not in the batch. 
                // This happens when Genetic algorithm uses a cached fitness instead of using the EvaluateFitness method.
                foreach (var chromosomeGroup in population.GroupBy(p => p))
                {
                    int populationCount = chromosomeGroup.Count();
                    TChromosome chromosome = chromosomeGroup.Key;

                    AgentLog? match = _agentLogBatch.FirstOrDefault(p => p.Chromosome.Equals(chromosome));

                    // Chromosome was previously evaluated, finds its log and create a new one for this generation. 
                    if (match is null)
                    {
                        var old = existingAgentLogs.FirstOrDefault(p => p.Chromosome.Equals(chromosome));

                        match = new AgentLog() // create a new log for output with the replaced generation number
                        {
                            Chromosome = chromosome, Fitness = chromosome.Fitness, Generation = currentGeneration,
                            GamesPlayed = old!.GamesPlayed, GamesWon = old!.GamesWon, MovesMade = old!.MovesMade
                        };
                        _agentLogBatch.Add(match);
                    }

                    match!.Count = populationCount;
                }

                existingAgentLogs.Reverse();

                // Add the accumulated logs
                existingAgentLogs.AddRange(_agentLogBatch);

                // Write back to the file
                File.WriteAllText(_agentLogFilePath, JsonSerializer.Serialize(existingAgentLogs, _jsonOptions));
            }
        }

        Console.WriteLine($"Chromosome Logged Count: {_agentLogBatch.Sum(p => p.Count)} Unique: {_agentLogBatch.Count}");

        // Clear the in-memory batch
        _agentLogBatch.Clear();
    }

    #endregion

    #region Data Access

    public List<TChromosome> LoadLastGeneration(out int generationNumber)
    {
        generationNumber = 0;
        GenerationLogDto? lastGeneration = ReadGenerationLogs().LastOrDefault();
        generationNumber = lastGeneration?.Generation ?? 0;

        if (lastGeneration == null)
            return new List<TChromosome>();

        List<AgentLog> lastGenerationAgents = ReadAllAgentLogs();

        var lastGenerationChromosomes = new List<TChromosome>();
        foreach (var agentLog in lastGenerationAgents.Where(p => p.Generation == lastGeneration.Generation))
        {
            for (int i = 0; i < agentLog.Count; i++)
            {
                lastGenerationChromosomes.Add((TChromosome)agentLog.Chromosome);
            }
        }

        return lastGenerationChromosomes;
    }

    public List<GenerationLogDto> ReadGenerationLogs()
    {
        lock (_generationLogLock)
        {
            if (!File.Exists(_generationLogFilePath))
                return new List<GenerationLogDto>();
            var text = File.ReadAllText(_generationLogFilePath);
            var generations = JsonSerializer.Deserialize<List<GenerationLogDto>>(text, _jsonOptions);
            return generations ?? new List<GenerationLogDto>();
        }
    }

    public List<AgentLog> ReadAllAgentLogs()
    {
        lock (_agentLogLock)
        {
            if (!File.Exists(_agentLogFilePath))
                return new List<AgentLog>();
            var text = File.ReadAllText(_agentLogFilePath);
            var agentLogs = JsonSerializer.Deserialize<List<AgentLog>>(text, _jsonOptions);
            return agentLogs ?? new List<AgentLog>();
        }
    }

    #endregion


}
