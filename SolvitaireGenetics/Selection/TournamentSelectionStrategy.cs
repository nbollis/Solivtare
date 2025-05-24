using SolvitaireCore;

namespace SolvitaireGenetics;

/// <summary>
/// Creates a tournament with x random chromosomes, and selects the best by fitness until the desired population count is acheived. 
/// </summary>
public class TournamentSelectionStrategy<TAgent, TChromosome> : ISelectionStrategy<TAgent, TChromosome>
    where TChromosome : Chromosome
    where TAgent : IGeneticAgent<TChromosome>
{
    public List<TAgent> Select(List<TAgent> population, int numberOfParents, GeneticAlgorithmParameters parameters, Random random)
    {
        var selectedParents = new List<TAgent>(numberOfParents);

        // Shuffle the population to ensure fairness
        var shuffledPopulation = population.OrderBy(_ => random.Next()).ToList();
        int currentIndex = 0;

        // Create tournaments
        for (int i = 0; i < numberOfParents; i++)
        {
            var tournament = new List<TAgent>(parameters.TournamentSize);

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