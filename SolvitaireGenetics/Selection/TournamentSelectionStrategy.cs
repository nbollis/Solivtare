using SolvitaireCore;

namespace SolvitaireGenetics;

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