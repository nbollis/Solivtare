using SolvitaireCore;

namespace SolvitaireGenetics;

public static class ReproductionStrategyFactory
{
    public static IReproductionStrategy<TChromosome> Create<TChromosome>(ReproductionStrategy strategy)
        where TChromosome : Chromosome, new()
    {
        return strategy switch
        {
            ReproductionStrategy.Sexual => new SexualReproductionStrategy<TChromosome>(),
            ReproductionStrategy.Asexual => new AsexualReproductionStrategy<TChromosome>(),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), $"Unknown reproduction strategy: {strategy}")
        };
    }
}