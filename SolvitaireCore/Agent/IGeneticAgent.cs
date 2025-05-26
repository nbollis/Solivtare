namespace SolvitaireCore;

public interface IGeneticAgent<TChromosome>
   where TChromosome : Chromosome
{
    TChromosome Chromosome { get; init; }
    double Fitness { get; set; }

    IGeneticAgent<TChromosome> CrossOver(IGeneticAgent<TChromosome> other, double crossoverRate = 0.5);
    IGeneticAgent<TChromosome> Mutate(double mutationRate);
    IGeneticAgent<TChromosome> Clone();
    public string GetStableHash() => Chromosome.GetStableHash();
}
