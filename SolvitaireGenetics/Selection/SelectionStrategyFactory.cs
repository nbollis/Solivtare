using SolvitaireCore;

namespace SolvitaireGenetics;

public static class SelectionStrategyFactory
{
    public static ISelectionStrategy<TChromosome> Create<TChromosome>(SelectionStrategy strategy)
        where TChromosome : Chromosome, new()
    {
        return strategy switch
        {
            SelectionStrategy.Tournament => new TournamentSelectionStrategy<TChromosome>(),
            SelectionStrategy.TopHalf => new TopHalfSelectionStrategy<TChromosome>(),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), $"Unknown selection strategy: {strategy}")
        };
    }
}