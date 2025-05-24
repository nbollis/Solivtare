using SolvitaireCore;

namespace SolvitaireGenetics;

public class SexualReproductionStrategy<TAgent, TChromosome> : IReproductionStrategy<TAgent, TChromosome>
    where TChromosome : Chromosome
    where TAgent : IGeneticAgent<TChromosome>
{
    public List<TAgent> Reproduce(List<TAgent> parents, int targetPopulation, double crossoverRate,
        double mutationRate, Random random)
    {
        var newPopulation = new List<TAgent>(targetPopulation);
        while (newPopulation.Count < targetPopulation)
        {
            for (int i = 0; i < parents.Count / 2; i++)
            {
                var parent1 = parents[i * 2];
                var parent2 = parents[i * 2 + 1];

                // Perform crossover and mutation
                var child = (TAgent)parent1.CrossOver(parent2, crossoverRate).Mutate(mutationRate);
                newPopulation.Add(child);
            }
        }

        // If too many, trim
        if (newPopulation.Count > targetPopulation)
            newPopulation = newPopulation.Take(targetPopulation).ToList();
        return newPopulation;
    }
}