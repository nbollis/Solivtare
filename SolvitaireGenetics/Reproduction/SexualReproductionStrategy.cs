using SolvitaireCore;

namespace SolvitaireGenetics;

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