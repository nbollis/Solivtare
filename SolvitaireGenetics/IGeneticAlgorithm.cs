namespace SolvitaireGenetics;

public interface IGeneticAlgorithm
{
    public event Action<int, GenerationLogDto>? GenerationCompleted;
    public event Action<AgentLog>? AgentCompleted;
    public Chromosome RunEvolution(int generations);
}