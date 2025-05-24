using SolvitaireCore;

namespace SolvitaireGenetics;

public interface ISelectionStrategy<TAgent, TChromosome>
    where TChromosome : Chromosome
    where TAgent : IGeneticAgent<TChromosome>
{
    List<TAgent> Select(List<TAgent> population, int numberOfParents, GeneticAlgorithmParameters parameters, Random random);
}