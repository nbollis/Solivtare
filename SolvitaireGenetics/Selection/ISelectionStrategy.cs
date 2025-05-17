using SolvitaireCore;

namespace SolvitaireGenetics;

public interface ISelectionStrategy<TChromosome>
    where TChromosome : Chromosome
{
    List<TChromosome> Select(List<TChromosome> population, int numberOfParents, GeneticAlgorithmParameters parameters, Random random);
}