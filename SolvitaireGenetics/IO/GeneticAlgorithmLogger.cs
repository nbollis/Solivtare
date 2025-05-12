using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using SolvitaireCore;
using SolvitaireIO;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithmLogger
{
    protected readonly object EventLock = new();
    protected int LastGenerationNumber = 0; // Last generation number logged
    protected readonly ConcurrentQueue<AgentLog> AgentLogBatch = new(); // In-memory batch for AgentLogs
    protected readonly string OutputDirectory;

    protected GeneticAlgorithmLogger(string? outputDirectory)
    {
        if (string.IsNullOrEmpty(outputDirectory))
            return;
        OutputDirectory = outputDirectory;

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

    }

    public abstract List<GenerationLog> ReadGenerationLogs();
    public abstract void LogGenerationInfo(GenerationLog generationLog);
    public abstract void LogChromosome(ChromosomeLog chromosome);
    public abstract List<AgentLog> ReadAllAgentLogs();
    public abstract void LogAgentDetail(AgentLog agentLog);
    public abstract void LogAgentDetails(IEnumerable<AgentLog> agentLogs);
    public void AccumulateAgentLog(AgentLog agentLog)
    {
        lock (EventLock)
        {
            AgentLogBatch.Enqueue(agentLog);
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

    public abstract List<AgentLog> LoadLastGeneration(out int generationNumber);


    public virtual void FlushAgentLogs<TChromosome>(int currentGeneration, List<TChromosome> population)
        where TChromosome : Chromosome, new()
    {
        List<AgentLog> agentLogsInBatch = new(population.Count);
        lock (EventLock)
        {
            while (AgentLogBatch.TryDequeue(out var log))
            {
                agentLogsInBatch.Add(log);
            }
        }
        List<AgentLog> agentLogsToWrite = new(population.Count);

        // Read existing agent logs 
        var existingAgentLogs = ReadAllAgentLogs();


        // reverse for easier lookup
        existingAgentLogs.Reverse();

        // Populate count information and ensure all chromosomes are represented in the log, even if they were not in the batch. 
        // This happens when Genetic algorithm uses a cached fitness instead of using the EvaluateFitness method.
        var grouped = population.GroupBy(p => p.GetStableHash());
        foreach (var chromosomeGroup in grouped)
        {
            int populationCount = chromosomeGroup.Count();
            TChromosome chromosome = chromosomeGroup.First();

            AgentLog? match = agentLogsInBatch.FirstOrDefault(p => p.Chromosome.Chromosome.Equals(chromosome));

            // Chromosome was previously evaluated, finds its log and create a new one for this generation. 
            if (match is null && currentGeneration > 0)
            {
                var old = existingAgentLogs.FirstOrDefault(p => p.Chromosome.Chromosome.Equals(chromosome));

                match = new AgentLog() // create a new log for output with the replaced generation number
                {
                    Chromosome = ChromosomeLog.FromChromosome(chromosome),
                    Fitness = chromosome.Fitness,
                    Generation = currentGeneration,
                    GamesPlayed = old!.GamesPlayed,
                    GamesWon = old!.GamesWon,
                    MovesMade = old!.MovesMade
                };
                agentLogsInBatch.Add(match);
            }

            match!.Count = populationCount;
            agentLogsToWrite.Add(match);
        }

        // Write back to the file
        LogAgentDetails(agentLogsToWrite);

        Console.WriteLine($"Chromosome Logged Count: {agentLogsInBatch.Sum(p => p.Count)} Unique: {agentLogsInBatch.Count}");
    }

    public virtual void CreateTsvSummaries(string outputDirectory)
    {
        string averagePath = Path.Combine(outputDirectory, "AverageChromosome.tsv");
        string bestPath = Path.Combine(outputDirectory, "BestChromosome.tsv");

        var generations = ReadGenerationLogs();
        var agentLogs = ReadAllAgentLogs();

        // Group agent logs by generation for efficient access
        var agentLogsByGeneration = agentLogs
            .GroupBy(log => log.Generation)
            .ToDictionary(group => group.Key, group => group.ToList());

        List<AgentLog> averageLog = new(generations.Count);
        List<AgentLog> bestLog = new(generations.Count);

        foreach (var generation in generations)
        {
            if (agentLogsByGeneration.TryGetValue(generation.Generation, out var logsForGeneration))
            {
                var best = logsForGeneration.FirstOrDefault(p => p.Chromosome.Chromosome.Equals(generation.BestChromosome.Chromosome));

                var avgLog = new AgentLog()
                {
                    Chromosome = generation.AverageChromosome,
                    Generation = generation.Generation,
                    Count = logsForGeneration.Average(p => p.Count),
                    Fitness = generation.AverageFitness,
                    GamesPlayed = logsForGeneration.Sum(p => p.GamesPlayed),
                    GamesWon = logsForGeneration.Sum(p => p.GamesWon),
                    MovesMade = logsForGeneration.Sum(p => p.MovesMade)
                };
                averageLog.Add(avgLog);

                if (best != null)
                {
                    bestLog.Add(best);
                }
            }
        }

        AgentLogTabFile.WriteToFile(averagePath, averageLog);
        AgentLogTabFile.WriteToFile(bestPath, bestLog);
    }
}

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

    public GeneticAlgorithmLogger(string? outputDirectory, bool ensureUniquePath = false) : base(outputDirectory)
    {
        if (string.IsNullOrEmpty(outputDirectory))
            return;

        _generationLogFilePath = Path.Combine(OutputDirectory, "GenerationalLog.json");
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

    public override void LogChromosome(ChromosomeLog chromosome)
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
        LastGenerationNumber = generationLog.Generation;

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

    public override void LogAgentDetails(IEnumerable<AgentLog> agentLogs)
    {
        if (_agentLogFilePath is null)
            return;

        lock (_agentLogLock)
        {
            // Read existing agent logs from the file  
            var existingAgentLogs = JsonSerializer.Deserialize<List<AgentLog>>(File.ReadAllText(_agentLogFilePath), _jsonOptions) ?? new List<AgentLog>();
            // Add the new agent log  
            existingAgentLogs.AddRange(agentLogs);
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
            Chromosome = ChromosomeLog.FromChromosome(chromosome),
            ChromosomeId = chromosome.GetStableHash()
        };

        AccumulateAgentLog(agentLog);
    }

    public override void FlushAgentLogs<TChromosomeBase>(int currentGeneration, List<TChromosomeBase> population)
    {
        if (_agentLogFilePath is null)
        {
            lock (EventLock)
            {
                AgentLogBatch.Clear(); // No file path, so just clear the batch and return
            }

            return; // No file path, so just clear the batch and return
        }

        base.FlushAgentLogs(currentGeneration, population);
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
