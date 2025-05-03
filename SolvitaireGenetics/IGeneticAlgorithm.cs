namespace SolvitaireGenetics;

public interface IGeneticAlgorithm
{
    public event Action<int, GenerationLogDto>? GenerationCompleted;
    public event Action<AgentLog>? AgentCompleted;
    public void RunEvolution(int generations);
    public void WriteParameters();
}