using System.Collections.Concurrent;
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
    private readonly Dictionary<int, ConcurrentBag<AgentLog>> _allChromosomesByGeneration = new();
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
        // Store the generational log in memory  
        _generationLog.Add(generationLog);
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
        if (!_allChromosomesByGeneration.TryGetValue(agentLog.Generation, out var chromosomes))
        {
            chromosomes = new ConcurrentBag<AgentLog>();
            _allChromosomesByGeneration[agentLog.Generation] = chromosomes;
        }
        chromosomes.Add(agentLog);

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
        // Add all accumulated logs to the generation cache
        foreach (var agentLog in _agentLogBatch)
        {
            if (!_allChromosomesByGeneration.TryGetValue(agentLog.Generation, out var chromosomes))
            {
                chromosomes = new ConcurrentBag<AgentLog>();
                _allChromosomesByGeneration[agentLog.Generation] = chromosomes;
            }
            chromosomes!.Add(agentLog);
        }

        var grouped = population.GroupBy(p => p.GetHashCode(), p => p);
        foreach (var chromosomeGroup in grouped)
        {
            int populationCount = chromosomeGroup.Count();
            TChromosome chromosome = chromosomeGroup.First();

            if (!_allChromosomesByGeneration.TryGetValue(currentGeneration, out var generationLogs))
            {
                generationLogs = new ConcurrentBag<AgentLog>();
                _allChromosomesByGeneration[currentGeneration] = generationLogs;
            }

            // Check if the chromosome is already logged for the current generation
            var existingLog = generationLogs.FirstOrDefault(log => log.Chromosome.Equals(chromosome));
            var loggedCount = generationLogs.Count(log => log.Chromosome.Equals(chromosome));
            
            if (loggedCount == populationCount) continue;

            // We found the match
            if (existingLog is not null)
            {
                for (int i = 0; i < populationCount - loggedCount; i++)
                {
                    _agentLogBatch.Add(existingLog);
                    generationLogs.Add(existingLog);
                }
            }
            // Did not find it in this generation. 
            else
            {
                for (int i = currentGeneration - 1; i > -1; i--)
                {
                    var innerMatch = _allChromosomesByGeneration[i].FirstOrDefault(p => p.Chromosome.Equals(chromosome));

                    if (innerMatch is null)
                        continue;

                    var log = new AgentLog()
                    {
                        Chromosome = innerMatch.Chromosome,
                        Generation = currentGeneration,
                        Fitness = innerMatch.Fitness,
                        GamesPlayed = innerMatch.GamesPlayed,
                        GamesWon = innerMatch.GamesWon,
                        MovesMade = innerMatch.MovesMade
                    };

                    for (int j = 0; j < populationCount; j++)
                    {
                        _agentLogBatch.Add(log);
                        _allChromosomesByGeneration[currentGeneration].Add(log);
                    }
                    break;
                }
            }
        }

        if (_agentLogFilePath is not null)
        {
            lock (_agentLogLock)
            {
                // Read existing agent logs from the file
                var existingAgentLogs =
                    JsonSerializer.Deserialize<List<AgentLog>>(File.ReadAllText(_agentLogFilePath), _jsonOptions) ??
                    new List<AgentLog>();

                // Add the accumulated logs
                existingAgentLogs.AddRange(_agentLogBatch);

                // Write back to the file
                File.WriteAllText(_agentLogFilePath, JsonSerializer.Serialize(existingAgentLogs, _jsonOptions));
            }
        }

        // Clear the in-memory batch
        _agentLogBatch.Clear();
    }

    #endregion

    #region Data Access

    public List<TChromosome> LoadLastGeneration(out int generationNumber)
    {
        generationNumber = _lastGenerationNumber;
        if (_allChromosomesByGeneration.TryGetValue(generationNumber, out var agentLogs))
        {
            return agentLogs.Select(p => p.Chromosome as TChromosome).ToList()!;
        }

        generationNumber = 0;
        GenerationLogDto? lastGeneration = LoadGenerationLogs().LastOrDefault();
        generationNumber = lastGeneration?.Generation ?? 0;

        if (lastGeneration == null)
            return new List<TChromosome>();

        List<AgentLog> lastGenerationAgents = LoadAllAgentLogs();

        var lastGenerationChromosomes = new List<TChromosome>();
        foreach (var agentLog in lastGenerationAgents.Where(p => p.Generation == lastGeneration.Generation))
        {
            lastGenerationChromosomes.Add((TChromosome)agentLog.Chromosome);
        }

        return lastGenerationChromosomes;
    }

    public List<GenerationLogDto> GetAllGenerationalLogs()
    {
        var toReturn =  new List<GenerationLogDto>(_generationLog);
        var expectedGenerations = Enumerable.Range(0, _lastGenerationNumber);
        if (toReturn.Select(p => p.Generation).SequenceEqual(expectedGenerations))
            return toReturn;

        // If the cache is empty, load from the file  
        return LoadGenerationLogs();
    }

    public Dictionary<int, List<AgentLog>> GetAllChromosomesByGeneration()
    {
        // Check if all generations up to the last generation number are present in the dictionary  
        if (_allChromosomesByGeneration.Count == _lastGenerationNumber + 1 &&
            Enumerable.Range(0, _lastGenerationNumber + 1).All(i => _allChromosomesByGeneration.ContainsKey(i)))
        {
            return _allChromosomesByGeneration.ToDictionary(p => p.Key, p => p.Value.ToList());
        }

        // If not, load from file  
        var allAgentLogs = LoadAllAgentLogs();
        foreach (var agentLog in allAgentLogs)
        {
            if (!_allChromosomesByGeneration.TryGetValue(agentLog.Generation, out var chromosomes))
            {
                chromosomes = new ConcurrentBag<AgentLog>();
                _allChromosomesByGeneration[agentLog.Generation] = chromosomes;
            }
            chromosomes.Add(agentLog);
        }

        return _allChromosomesByGeneration.ToDictionary(p => p.Key, p => p.Value.ToList());
    }

    internal List<GenerationLogDto> LoadGenerationLogs()
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

    internal List<AgentLog> LoadAllAgentLogs()
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
