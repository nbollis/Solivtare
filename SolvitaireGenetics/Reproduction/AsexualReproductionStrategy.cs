using SolvitaireCore;

namespace SolvitaireGenetics;

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