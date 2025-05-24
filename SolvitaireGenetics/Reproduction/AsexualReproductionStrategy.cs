using SolvitaireCore;

namespace SolvitaireGenetics;

public class AsexualReproductionStrategy<TAgent, TChromosome> : IReproductionStrategy<TAgent, TChromosome>
    where TChromosome : Chromosome
    where TAgent : IGeneticAgent<TChromosome>
{

    public List<TAgent> Reproduce(List<TAgent> parents, int targetPopulation, double crossoverRate,
        double mutationRate, Random random)
    {
        var newPopulation = new List<TAgent>(targetPopulation);
        while (newPopulation.Count < targetPopulation)
        {
            foreach (var parent in parents)
            {
                var child = (TAgent)parent.Mutate(mutationRate);
                newPopulation.Add(child);
            }
        }

        // trim if needed to get the count right
        if (newPopulation.Count > targetPopulation)
            newPopulation = newPopulation.Take(targetPopulation).ToList();
        return newPopulation;
    }
}