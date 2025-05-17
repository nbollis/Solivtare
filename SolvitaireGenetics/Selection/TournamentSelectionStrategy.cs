using System.Security.Cryptography.X509Certificates;
using SolvitaireCore;

namespace SolvitaireGenetics;

/// <summary>
/// Creates a tournament with x random chromosomes, and selects the best by fitness until the desired population count is acheived. 
/// </summary>
/// <typeparam name="TChromosome"></typeparam>
public class TournamentSelectionStrategy<TChromosome> : ISelectionStrategy<TChromosome> where TChromosome : Chromosome
{
    public List<TChromosome> Select(List<TChromosome> population, int numberOfParents, GeneticAlgorithmParameters parameters, Random random)
    {
        var selectedParents = new List<TChromosome>(numberOfParents);

        // Shuffle the population to ensure fairness
        var shuffledPopulation = population.OrderBy(_ => random.Next()).ToList();
        int currentIndex = 0;

        // Create tournaments
        for (int i = 0; i < numberOfParents; i++)
        {
            var tournament = new List<TChromosome>(parameters.TournamentSize);

            // Select TournamentSize chromosomes from the shuffled population
            for (int j = 0; j < parameters.TournamentSize; j++)
            {
                // If we reach the end of the shuffled population, reshuffle and reset the index
                if (currentIndex >= shuffledPopulation.Count)
                {
                    shuffledPopulation = population.OrderBy(_ => random.Next()).ToList();
                    currentIndex = 0;
                }

                tournament.Add(shuffledPopulation[currentIndex]);
                currentIndex++;
            }

            // Select the winner based on fitness
            var winner = tournament
                .OrderByDescending(chromosome => chromosome.Fitness)
                .First();

            selectedParents.Add(winner);
        }

        return selectedParents;
    }
}

/// <summary>
/// A rank-based fitness scaling where selection probability is proportional to absolute fitness.
/// The output is a resampled population (not just selected parents). The strategy is fitness replication based on fitness counts.
/// A well fit chromosome may be duplicated multiple times, a less fit may not occur
/// </summary>
public class FitnessScaledSelectionStrategy<TChromosome> : ISelectionStrategy<TChromosome>
    where TChromosome : Chromosome
{
    public List<TChromosome> Select(List<TChromosome> population, int numberOfParents,
         GeneticAlgorithmParameters parameters, Random random)
    {
        var newPopulation = new List<TChromosome>(numberOfParents);

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
        var expectedCounts = new Dictionary<TChromosome, double>();
        foreach (var chrom in population)
        {
            double expected = (chrom.Fitness / totalFitness) * numberOfParents;
            expectedCounts[chrom] = expected;
        }

        // First, floor the expected counts and assign those
        var fractionalParts = new List<(TChromosome chrom, double fractional)>();
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