using SolvitaireCore;

namespace SolvitaireGenetics;

public interface IReproductionStrategy<TChromosome>
    where TChromosome : Chromosome
{
    List<TChromosome> Reproduce(List<TChromosome> parents, int targetPopulation, double crossoverRate, double mutationRate, Random random);
}