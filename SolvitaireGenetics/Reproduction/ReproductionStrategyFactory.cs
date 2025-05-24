using SolvitaireCore;

namespace SolvitaireGenetics;

public static class ReproductionStrategyFactory
{
    public static IReproductionStrategy<TAgent, TChromosome> Create<TAgent,TChromosome>(ReproductionStrategy strategy)
        where TChromosome : Chromosome, new()
        where TAgent : IGeneticAgent<TChromosome>
    {
        return strategy switch
        {
            ReproductionStrategy.Sexual => new SexualReproductionStrategy<TAgent, TChromosome>(),
            ReproductionStrategy.Asexual => new AsexualReproductionStrategy<TAgent, TChromosome>(),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), $"Unknown reproduction strategy: {strategy}")
        };
    }
}