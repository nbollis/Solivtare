namespace SolvitaireCore;

public interface IAgent<in TGameState>
{
    string Name { get; }
    public AgentDecision GetNextAction(TGameState gameState);
}

public interface IGeneticAgent<TChromosome, TGameState>
    : IAgent<TGameState>
    where TChromosome : Chromosome
{
    double Fitness { get; set; }
    IReadOnlyList<TChromosome> Chromosomes { get; }
    IGeneticAgent<TChromosome, TGameState> Crossover(IGeneticAgent<TChromosome, TGameState> other, double crossoverRate);
    IGeneticAgent<TChromosome, TGameState> Mutate(double mutationRate);
}