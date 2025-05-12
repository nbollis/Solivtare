using SolvitaireIO.Database;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public class DatabaseGeneticAlgorithmLogger : GeneticAlgorithmLogger
{
    private readonly IRepositoryManager _repositoryManager;

    public DatabaseGeneticAlgorithmLogger(DbContextFactory dbContextFactory, IRepositoryManager? repositoryManager = null) : base(dbContextFactory.OutputDirectory)
    {
        _repositoryManager = repositoryManager ?? new RepositoryManager(dbContextFactory);
    }

    public override List<GenerationLog> ReadGenerationLogs()
    {
        var generationRepository = _repositoryManager.CreateGenerationRepository();
        return generationRepository.GetAllGenerationLogsAsync().Result;
    }

    public override void LogGenerationInfo(GenerationLog generationLog)
    {
        LastGenerationNumber = generationLog.Generation;
        var generationRepository = _repositoryManager.CreateGenerationRepository();
        generationRepository.AddGenerationAsync(generationLog).Wait();
    }

    public override void LogChromosome(ChromosomeLog chromosome)
    {
        var chromosomeRepository = _repositoryManager.CreateChromosomeRepository();
        chromosomeRepository.AddChromosomeLogAsync(chromosome).Wait();
    }

    public override List<AgentLog> ReadAllAgentLogs()
    {
        var agentRepository = _repositoryManager.CreateAgentRepository();
        return agentRepository.GetAgentLogsByGenerationAsync().Result;
    }

    public override void LogAgentDetail(AgentLog agentLog)
    {
        var agentRepository = _repositoryManager.CreateAgentRepository();
        agentRepository.AddAgentAsync(agentLog).Wait();
    }

    public override void LogAgentDetails(IEnumerable<AgentLog> agentLogs)
    {
        foreach (var agentLog in agentLogs)
        {
            LogAgentDetail(agentLog);
        }
    }

    public override List<AgentLog> LoadLastGeneration(out int generationNumber)
    {
        // Retrieve the last generation and its associated agent logs
        var generationRepository = _repositoryManager.CreateGenerationRepository();
        var agentRepository = _repositoryManager.CreateAgentRepository();

        var lastGeneration = generationRepository.GetLastGenerationAsync().Result;
        generationNumber = lastGeneration?.Generation ?? 0;

        return lastGeneration != null
            ? agentRepository.GetAgentLogsByGenerationAsync(lastGeneration.Generation).Result
            : new List<AgentLog>();
    }
}
