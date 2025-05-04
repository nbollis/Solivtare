namespace SolvitaireGenetics;

public interface IGeneticAlgorithm
{
    public event Action<int, GenerationLogDto>? GenerationCompleted;
    public event Action<AgentLog>? AgentCompleted; 
    public IGeneticAlgorithmLogger Logger { get; }
    public Chromosome RunEvolution(int generations);
}

public interface IGeneticAlgorithmLogger
{
    public List<GenerationLogDto> ReadGenerationLogs();
    public void LogGenerationInfo(GenerationLogDto generationLog);

    public List<AgentLog> ReadAllAgentLogs();
    public void LogAgentDetail(AgentLog agentLog);
    public void AccumulateAgentLog(AgentLog agentLog);

    public void SubscribeToAlgorithm(IGeneticAlgorithm algorithm)
    {
        algorithm.GenerationCompleted += (generation, generationLog) =>
        {
            LogGenerationInfo(generationLog);
        };

        algorithm.AgentCompleted += AccumulateAgentLog;
    }

    public List<AgentLog> LoadLastGeneration(out int generationNumber);

    public void FlushAgentLogs<TChromosome>(int currentGeneration, List<TChromosome> population)
        where TChromosome : Chromosome, new();
}