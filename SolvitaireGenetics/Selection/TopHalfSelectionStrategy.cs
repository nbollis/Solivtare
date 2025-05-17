using SolvitaireCore;

namespace SolvitaireGenetics;

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