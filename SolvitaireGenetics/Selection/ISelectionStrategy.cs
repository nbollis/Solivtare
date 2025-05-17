using SolvitaireCore;

namespace SolvitaireGenetics;

public interface ISelectionStrategy<TChromosome>
    where TChromosome : Chromosome
{
    List<TChromosome> Select(List<TChromosome> population, int numberOfParents, GeneticAlgorithmParameters parameters, Random random);
}

public enum SelectionStrategy
{
    Tournament,
    TopHalf
}

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
/// Takes to top half of the population by fitness and clones them to the next generation up to number of parents
/// </summary>
/// <typeparam name="TChromosome"></typeparam>
public class TopHalfSelectionStrategy<TChromosome> : ISelectionStrategy<TChromosome> where TChromosome : Chromosome
{
    public List<TChromosome> Select(List<TChromosome> population, int numberOfParents, GeneticAlgorithmParameters parameters, Random random)
    {
        // Sort the population by fitness in descending order
        var sortedPopulation = population.OrderByDescending(chromosome => chromosome.Fitness).ToList();

        // Select the top half of the population
        var topHalf = sortedPopulation.Take(sortedPopulation.Count / 2).ToList();
        var selectedParents = new List<TChromosome>();

        // Clone the top half to fill the number of parents
        while (selectedParents.Count < numberOfParents)
        {
            foreach (var parent in topHalf)
            {
                // Clone the parent and add to the selected parents
                selectedParents.Add(parent);

                // Stop if we have enough parents
                if (selectedParents.Count >= numberOfParents)
                    break;
            }
        }

        // If we have more parents than needed, trim the list
        if (selectedParents.Count != numberOfParents)
        {
            selectedParents = selectedParents.Take(numberOfParents).ToList();
        }

        return selectedParents;
    }
}

