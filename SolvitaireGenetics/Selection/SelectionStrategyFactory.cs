using SolvitaireCore;

namespace SolvitaireGenetics;

public static class SelectionStrategyFactory
{
    public static ISelectionStrategy<TAgent, TChromosome> Create<TAgent, TChromosome>(SelectionStrategy strategy)
        where TChromosome : Chromosome, new()
        where TAgent : IGeneticAgent<TChromosome>
    {
        return strategy switch
        {
            SelectionStrategy.Tournament => new TournamentSelectionStrategy<TAgent, TChromosome>(),
            SelectionStrategy.TopHalf => new TopHalfSelectionStrategy<TAgent, TChromosome>(),
            SelectionStrategy.FitnessScaled => new FitnessScaledSelectionStrategy<TAgent, TChromosome>(),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), $"Unknown selection strategy: {strategy}")
        };
    }
}