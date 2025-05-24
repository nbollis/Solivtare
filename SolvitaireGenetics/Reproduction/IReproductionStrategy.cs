using SolvitaireCore;

namespace SolvitaireGenetics;

public interface IReproductionStrategy<TAgent, TChromosome>
    where TChromosome : Chromosome
    where TAgent : IGeneticAgent<TChromosome>
{
    List<TAgent> Reproduce(List<TAgent> parents, int targetPopulation, double crossoverRate, double mutationRate, Random random);
}