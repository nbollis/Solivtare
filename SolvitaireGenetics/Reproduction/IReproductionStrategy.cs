using SolvitaireCore;

namespace SolvitaireGenetics;

public interface IReproductionStrategy<TChromosome>
    where TChromosome : Chromosome
{
    List<TChromosome> Reproduce(List<TChromosome> parents, int targetPopulation, double crossoverRate, double mutationRate, Random random);
}

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

public enum ReproductionStrategy
{
    Sexual, 
    Asexual
}

public class SexualReproductionStrategy<TChromosome> : IReproductionStrategy<TChromosome> where TChromosome : Chromosome
{
    public List<TChromosome> Reproduce(List<TChromosome> parents, int targetPopulation, double crossoverRate,
        double mutationRate, Random random)
    {
        var newPopulation = new List<TChromosome>(targetPopulation);
        while (newPopulation.Count < targetPopulation)
        {
            for (int i = 0; i < parents.Count / 2; i++)
            {
                var parent1 = parents[i * 2];
                var parent2 = parents[i * 2 + 1];

                // Perform crossover and mutation
                var child = Chromosome.Crossover(parent1, parent2, crossoverRate);
                child = Chromosome.Mutate(child, mutationRate);

                newPopulation.Add(child);
            }
        }

        // If too many, trim
        if (newPopulation.Count > targetPopulation)
            newPopulation = newPopulation.Take(targetPopulation).ToList();
        return newPopulation;
    }
}

public class AsexualReproductionStrategy<TChromosome> : IReproductionStrategy<TChromosome> where TChromosome : Chromosome
{

    public List<TChromosome> Reproduce(List<TChromosome> parents, int targetPopulation, double crossoverRate,
        double mutationRate, Random random)
    {
        var newPopulation = new List<TChromosome>(targetPopulation);
        while (newPopulation.Count < targetPopulation)
        {
            foreach (var parent in parents)
            {
                var child = parent.Mutate<TChromosome>(mutationRate);
                newPopulation.Add(child);
            }
        }

        // trim if needed to get the count right
        if (newPopulation.Count > targetPopulation)
            newPopulation = newPopulation.Take(targetPopulation).ToList();
        return newPopulation;
    }
}