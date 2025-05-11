using SolvitaireCore;
using SolvitaireIO.Database;
using SolvitaireIO.Database.Models;
using SolvitaireIO.Database.Repositories;

namespace SolvitaireGenetics;

public class DatabaseGeneticAlgorithmLogger : IGeneticAlgorithmLogger
{
    private readonly IRepositoryManager _repositoryManager;

    public DatabaseGeneticAlgorithmLogger(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public List<GenerationLog> ReadGenerationLogs()
    {
        // Retrieve all generation logs from the database
        return _repositoryManager.GenerationRepository.GetAllGenerationLogsAsync().Result;
    }

    public void LogGenerationInfo(GenerationLog generationLog)
    {
        // Add the generation log to the database
        _repositoryManager.GenerationRepository.AddGenerationAsync(generationLog).Wait();
        _repositoryManager.SaveChangesAsync().Wait();
    }

    public List<AgentLog> ReadAllAgentLogs()
    {
        // Retrieve all agent logs from the database
        return _repositoryManager.AgentRepository.GetAgentLogsByGenerationAsync(-1).Result; // -1 to get all logs
    }

    public void LogAgentDetail(AgentLog agentLog)
    {
        // Add the agent log to the database
        _repositoryManager.AgentRepository.AddAgentAsync(agentLog).Wait();
        _repositoryManager.SaveChangesAsync().Wait();
    }

    public void AccumulateAgentLog(AgentLog agentLog)
    {
        // Add or update the agent log in the database
        LogAgentDetail(agentLog);
    }

    public List<AgentLog> LoadLastGeneration(out int generationNumber)
    {
        // Retrieve the last generation and its associated agent logs
        var lastGeneration = _repositoryManager.GenerationRepository.GetLastGenerationAsync().Result;
        generationNumber = lastGeneration?.Generation ?? 0;

        return lastGeneration != null
            ? _repositoryManager.AgentRepository.GetAgentLogsByGenerationAsync(lastGeneration.Generation).Result
            : new List<AgentLog>();
    }

    public void FlushAgentLogs<TChromosome>(int currentGeneration, List<TChromosome> population)
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

    public void CreateTsvSummaries(string outputDirectory)
    {
        // Use the existing logic to create TSV summaries
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
                var best = logsForGeneration.FirstOrDefault(p => p.Chromosome.StableHash == generation.BestChromosome?.StableHash);

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
