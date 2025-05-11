using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using SolvitaireCore;
using SolvitaireIO;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

/// <summary>  
/// Logs Generational and Agent specific data throughout a genetic algorithm run.   
/// </summary>  
/// <typeparam name="TChromosome"></typeparam>  
public class GeneticAlgorithmLogger<TChromosome> : GeneticAlgorithmLogger 
    where TChromosome : Chromosome, new()
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string? _generationLogFilePath;
    private readonly string? _agentLogFilePath;
    private readonly object _generationLogLock = new();
    private readonly object _agentLogLock = new();

    private readonly object _eventLock = new();
    private int _lastGenerationNumber = 0; // Last generation number logged
    private readonly ConcurrentQueue<AgentLog> _agentLogBatch = new(); // In-memory batch for AgentLogs
    internal readonly string OutputDirectory;

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



    #region Log Directly to File/Memory

    public override void LogChromosome(Chromosome chromosome)
    {
        // Do nothing, this is taken care of in the generational and agent logs. 
    }

    /// <summary>  
    /// Logs generational information, including best and average fitness and chromosomes.  
    /// </summary>  
    public void LogGenerationInfo(int generation, double bestFitness, double averageFitness, double stdFitness, TChromosome bestChromosome, TChromosome averageChromosome, TChromosome stdChromosome)
    {
        var generationLog = new GenerationLog
        {
            Generation = generation,
            BestFitness = bestFitness,
            AverageFitness = averageFitness,
            StdFitness = stdFitness,
            BestChromosomeId = bestChromosome.GetStableHash(),
            AverageChromosomeId = averageChromosome.GetStableHash(),
            StdChromosomeId = stdChromosome.GetStableHash(),
        };

        LogGenerationInfo(generationLog);
    }

    public override void LogGenerationInfo(GenerationLog generationLog)
    {
        _lastGenerationNumber = generationLog.Generation;

        if (_generationLogFilePath is null)
            return;

        lock (_generationLogLock)
        {
            // Read existing generations from the file  
            var existingGenerations = JsonSerializer.Deserialize<List<GenerationLog>>(File.ReadAllText(_generationLogFilePath), _jsonOptions) ?? new List<GenerationLog>();

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
            Chromosome = ChromosomeLog.FromChromosome(chromosome)
        };

        LogAgentDetail(agentLog);
    }

    /// <summary>  
    /// Logs detailed information for a single agent in the generation.  
    /// </summary>  
    public override void LogAgentDetail(AgentLog agentLog)
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
            Chromosome = ChromosomeLog.FromChromosome(chromosome)
        };

        AccumulateAgentLog(agentLog);
    }

    /// <summary>
    /// Accumulates detailed information for a single agent in the generation. Write the batch with FlushAgentLogs.
    /// </summary>
    public override void AccumulateAgentLog(AgentLog agentLog)
    {
        lock (_eventLock)
        {
            _agentLogBatch.Enqueue(agentLog);
        }
    }

    

    #endregion

    #region Data Access

    public override List<AgentLog> LoadLastGeneration(out int generationNumber)
    {
        generationNumber = 0;
        GenerationLog? lastGeneration = ReadGenerationLogs().LastOrDefault();
        generationNumber = lastGeneration?.Generation ?? 0;

        if (lastGeneration == null)
            return new List<AgentLog>();

        List<AgentLog> lastGenerationAgents = ReadAllAgentLogs()
            .Where(p => p.Generation == lastGeneration.Generation)
            .SelectMany(p => Enumerable.Repeat(p, (int)p.Count))
            .ToList();

        return lastGenerationAgents;
    }

    public override List<GenerationLog> ReadGenerationLogs()
    {
        lock (_generationLogLock)
        {
            if (!File.Exists(_generationLogFilePath))
                return new List<GenerationLog>();
            var text = File.ReadAllText(_generationLogFilePath);
            var generations = JsonSerializer.Deserialize<List<GenerationLog>>(text, _jsonOptions);
            return generations ?? new List<GenerationLog>();
        }
    }

    public override List<AgentLog> ReadAllAgentLogs()
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
