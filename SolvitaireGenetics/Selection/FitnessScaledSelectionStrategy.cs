using SolvitaireCore;

namespace SolvitaireGenetics;

/// <summary>
/// A rank-based fitness scaling where selection probability is proportional to absolute fitness.
/// The output is a resampled population (not just selected parents). The strategy is fitness replication based on fitness counts.
/// A well fit chromosome may be duplicated multiple times, a less fit may not occur
/// </summary>
public class FitnessScaledSelectionStrategy<TAgent, TChromosome> : ISelectionStrategy<TAgent, TChromosome>
    where TChromosome : Chromosome
    where TAgent : IGeneticAgent<TChromosome>
{
    public List<TAgent> Select(List<TAgent> population, int numberOfParents,
        GeneticAlgorithmParameters parameters, Random random)
    {
        var newPopulation = new List<TAgent>(numberOfParents);

        // Total fitness for scaling
        double totalFitness = population.Sum(p => p.Fitness);

        if (totalFitness <= 0)
        {
            // Fall back to uniform random selection if all fitness is non-positive
            for (int i = 0; i < numberOfParents; i++)
            {
                newPopulation.Add(population[random.Next(population.Count)]);
            }
            return newPopulation;
        }

        // Calculate expected count per chromosome
        var expectedCounts = new Dictionary<TAgent, double>();
        foreach (var chrom in population)
        {
            double expected = (chrom.Fitness / totalFitness) * numberOfParents;
            expectedCounts[chrom] = expected;
        }

        // First, floor the expected counts and assign those
        var fractionalParts = new List<(TAgent chrom, double fractional)>();
        int assigned = 0;
        foreach (var kvp in expectedCounts)
        {
            int count = (int)Math.Floor(kvp.Value);
            assigned += count;
            for (int i = 0; i < count; i++)
            {
                newPopulation.Add(kvp.Key);
            }

            double fractional = kvp.Value - count;
            if (fractional > 0)
                fractionalParts.Add((kvp.Key, fractional));
        }

        // Fill remaining slots using stochastic rounding from fractional parts
        int remaining = numberOfParents - assigned;
        fractionalParts = fractionalParts
            .OrderByDescending(fp => fp.fractional)
            .ToList();

        for (int i = 0; i < remaining && i < fractionalParts.Count; i++)
        {
            newPopulation.Add(fractionalParts[i].chrom);
        }

        return newPopulation;
    }
}