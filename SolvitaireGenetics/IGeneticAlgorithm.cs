using SolvitaireCore;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public interface IGeneticAlgorithm
{
    public event Action<int, GenerationLog>? GenerationCompleted;
    public event Action<AgentLog>? AgentCompleted;
    public GeneticAlgorithmLogger Logger { get; }
    public Chromosome RunEvolution(int generations, CancellationToken? cancellationToken = null);
}

public abstract class GeneticAlgorithmLogger
{
    public abstract List<GenerationLog> ReadGenerationLogs();
    public abstract void LogGenerationInfo(GenerationLog generationLog);
    public abstract void LogChromosome(Chromosome chromosome);
    public abstract List<AgentLog> ReadAllAgentLogs();
    public abstract void LogAgentDetail(AgentLog agentLog);
    public abstract void AccumulateAgentLog(AgentLog agentLog);

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
        foreach (var chromosome in population)
        {
            var agentLog = new AgentLog
            {
                Generation = currentGeneration,
                Fitness = chromosome.Fitness,
                Chromosome = ChromosomeLog.FromChromosome(chromosome)
            };

            LogAgentDetail(agentLog);
        }
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
                var best = logsForGeneration.FirstOrDefault(p => p.Chromosome.Equals(generation.BestChromosome));

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