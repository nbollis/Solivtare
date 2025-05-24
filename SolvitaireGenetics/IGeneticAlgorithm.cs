using SolvitaireCore;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public interface IGeneticAlgorithm
{
    public event Action<int, GenerationLog>? GenerationCompleted;
    public event Action<AgentLog>? AgentCompleted; 
    bool ThanosSnapTriggered { get; set; }
    GeneticAlgorithmLogger Logger { get; }
    public Chromosome RunEvolution(int generations, CancellationToken? cancellationToken = null);
}