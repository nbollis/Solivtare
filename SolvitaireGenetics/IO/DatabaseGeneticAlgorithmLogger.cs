using SolvitaireCore;
using SolvitaireIO.Database;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public class DatabaseGeneticAlgorithmLogger : GeneticAlgorithmLogger
{
    private readonly IRepositoryManager _repositoryManager;

    public DatabaseGeneticAlgorithmLogger(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public override List<GenerationLog> ReadGenerationLogs()
    {
        // Retrieve all generation logs from the database
        return _repositoryManager.GenerationRepository.GetAllGenerationLogsAsync().Result;
    }

    public override void LogGenerationInfo(GenerationLog generationLog)
    {
        // Add the generation log to the database
        _repositoryManager.GenerationRepository.AddGenerationAsync(generationLog).Wait();
        _repositoryManager.SaveChangesAsync().Wait();
    }

    public override void LogChromosome(Chromosome chromosome)
    {
        _repositoryManager.ChromosomeRepository.AddChromosomeAsync(chromosome).Wait();
    }

    public override List<AgentLog> ReadAllAgentLogs()
    {
        // Retrieve all agent logs from the database
        return _repositoryManager.AgentRepository.GetAgentLogsByGenerationAsync(-1).Result; // -1 to get all logs
    }

    public override void LogAgentDetail(AgentLog agentLog)
    {
        // Add the agent log to the database
        _repositoryManager.AgentRepository.AddAgentAsync(agentLog).Wait();
        _repositoryManager.SaveChangesAsync().Wait();
    }

    public override void AccumulateAgentLog(AgentLog agentLog)
    {
        // Add or update the agent log in the database
        LogAgentDetail(agentLog);
    }

    public override List<AgentLog> LoadLastGeneration(out int generationNumber)
    {
        // Retrieve the last generation and its associated agent logs
        var lastGeneration = _repositoryManager.GenerationRepository.GetLastGenerationAsync().Result;
        generationNumber = lastGeneration?.Generation ?? 0;

        return lastGeneration != null
            ? _repositoryManager.AgentRepository.GetAgentLogsByGenerationAsync(lastGeneration.Generation).Result
            : new List<AgentLog>();
    }
}
