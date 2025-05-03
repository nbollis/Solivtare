using System.Text.Json;
using SolvitaireIO;

namespace SolvitaireGenetics;

/// <summary>  
/// Logs Generational and Agent specific data throughout a genetic algorithm run.   
/// </summary>  
/// <typeparam name="TChromosome"></typeparam>  
public class GeneticAlgorithmLogger<TChromosome> where TChromosome : Chromosome
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _generationLogFilePath;
    private readonly string _agentLogFilePath;
    private readonly object _generationLogLock = new();
    private readonly object _agentLogLock = new();
    protected readonly List<AgentLog> AgentLogBatch = new(); // In-memory batch for AgentLogs
    internal readonly string OutputDirectory;

    public GeneticAlgorithmLogger(string outputDirectory, bool ensureUniquePath = false)
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

    public void SubscribeToAlgorithm(IGeneticAlgorithm algorithm)
    {
        algorithm.GenerationCompleted += (generation, generationLog) =>
        {
            LogGenerationInfo(generationLog);
        };

        algorithm.AgentCompleted += AccumulateAgentLog;
    }

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
            BestChromosome = new ChromosomeDto { Weights = bestChromosome.MutableStatsByName },
            AverageChromosome = new ChromosomeDto { Weights = averageChromosome.MutableStatsByName },
            StdChromosome = new ChromosomeDto { Weights = stdChromosome.MutableStatsByName }
        };

        LogGenerationInfo(generationLog);
    }

    public virtual void LogGenerationInfo(GenerationLogDto generationLog)
    {
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
            Chromosome = new ChromosomeDto { Weights = chromosome.MutableStatsByName }
        };

        LogAgentDetail(agentLog);
    }

    /// <summary>  
    /// Logs detailed information for a single agent in the generation.  
    /// </summary>  
    public virtual void LogAgentDetail(AgentLog agentLog)
    {
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
            Chromosome = new ChromosomeDto { Weights = chromosome.MutableStatsByName }
        };

        lock (_agentLogLock)
        {
            AgentLogBatch.Add(agentLog);
        }
    }

    /// <summary>
    /// Accumulates detailed information for a single agent in the generation. Write the batch with FlushAgentLogs.
    /// </summary>
    public virtual void AccumulateAgentLog(AgentLog agentLog)
    {
        lock (_agentLogLock)
        {
            AgentLogBatch.Add(agentLog);
        }
    }

    /// <summary>
    /// Writes out the accumulated AgentLogs to the file and clears the in-memory batch.
    /// </summary>
    public virtual void FlushAgentLogs()
    {
        lock (_agentLogLock)
        {
            // Read existing agent logs from the file
            var existingAgentLogs = JsonSerializer.Deserialize<List<AgentLog>>(File.ReadAllText(_agentLogFilePath), _jsonOptions) ?? new List<AgentLog>();

            // Add the accumulated logs
            existingAgentLogs.AddRange(AgentLogBatch);

            // Write back to the file
            File.WriteAllText(_agentLogFilePath, JsonSerializer.Serialize(existingAgentLogs, _jsonOptions));

            // Clear the in-memory batch
            AgentLogBatch.Clear();
        }
    }

    public virtual List<TChromosome> LoadLastGeneration(out int generationNumber)
    {
        generationNumber = 0;
        GenerationLogDto? lastGeneration = null;
        lock (_generationLogLock)
        {
            if (!File.Exists(_generationLogFilePath))
                return new List<TChromosome>();
            var text = File.ReadAllText(_generationLogFilePath);
            var generations = JsonSerializer.Deserialize<List<GenerationLogDto>>(text, _jsonOptions);
            lastGeneration = generations?.LastOrDefault();
            generationNumber = lastGeneration?.Generation ?? 0;
        }

        if (lastGeneration == null)
            return new List<TChromosome>();

        List<AgentLog> lastGenerationAgents = new();
        lock (_agentLogLock)
        {
            if (!File.Exists(_agentLogFilePath))
                return new List<TChromosome>();
            lastGenerationAgents = JsonSerializer.Deserialize<List<AgentLog>>(File.ReadAllText(_agentLogFilePath), _jsonOptions) ?? new List<AgentLog>();
        }

        var lastGenerationChromosomes = new List<TChromosome>();
        foreach (var agentLog in lastGenerationAgents.Where(p => p.Generation == lastGeneration.Generation))
        {
            var chromosome = Activator.CreateInstance<TChromosome>();
            chromosome.MutableStatsByName = agentLog.Chromosome.Weights;
            lastGenerationChromosomes.Add(chromosome);
        }

        return lastGenerationChromosomes;
    }
}


/// <summary>
/// A lightweight logger that only keeps the last generation of chromosomes in memory.
/// </summary>
/// <typeparam name="TChromosome"></typeparam>
public class InMemoryGeneticAlgorithmLogger<TChromosome> : GeneticAlgorithmLogger<TChromosome>
  where TChromosome : Chromosome
{
    private readonly Dictionary<int, List<TChromosome>> _allChromosomesByGeneration = new();
    private readonly List<GenerationLogDto> _generationLog = new();
    private int _lastGenerationNumber;

    public InMemoryGeneticAlgorithmLogger() : base(outputDirectory: string.Empty, ensureUniquePath: false)
    {
        // Disable file-based logging by passing an empty string for the output directory  
    }

    /// <summary>  
    /// Overrides the method to store only the last generation in memory.  
    /// </summary>  
    public override void LogGenerationInfo(GenerationLogDto generationLog)
    {
        // Store the generational log in memory  
        _generationLog.Add(generationLog);
        _lastGenerationNumber = generationLog.Generation;
    }

    /// <summary>  
    /// Accumulates the last generation of chromosomes in memory.  
    /// </summary>  
    public override void AccumulateAgentLog(AgentLog agentLog)
    {
        AgentLogBatch.Add(agentLog);
    }

    public override void LogAgentDetail(AgentLog agentLog)
    {
        _allChromosomesByGeneration[agentLog.Generation] = _allChromosomesByGeneration.TryGetValue(agentLog.Generation, out var value)
            ? value
            : [];

        TChromosome chromosome = Activator.CreateInstance<TChromosome>();
        chromosome.MutableStatsByName = agentLog.Chromosome.Weights;
        chromosome.Fitness = agentLog.Fitness;
        _allChromosomesByGeneration[agentLog.Generation].Add(chromosome);
    }

    public override void FlushAgentLogs()
    {
        foreach (var agentLog in AgentLogBatch)
        {
            TChromosome chromosome = Activator.CreateInstance<TChromosome>();
            chromosome.MutableStatsByName = agentLog.Chromosome.Weights;
            chromosome.Fitness = agentLog.Fitness;

            if (_allChromosomesByGeneration.TryGetValue(agentLog.Generation, out var value))
            {
                value.Add(chromosome);
            }
            else
            {
                _allChromosomesByGeneration[agentLog.Generation] = [chromosome];
            }
        }
        AgentLogBatch.Clear();
    }

    public override List<TChromosome> LoadLastGeneration(out int generationNumber)
    {
        generationNumber = _lastGenerationNumber;
        if (_allChromosomesByGeneration.TryGetValue(generationNumber, out var agentLogs))
        {
            return agentLogs;
        }
        return new List<TChromosome>();
    }
}

