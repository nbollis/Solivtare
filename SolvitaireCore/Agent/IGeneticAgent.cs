namespace SolvitaireCore;

public interface IGeneticAgent<TChromosome>
   where TChromosome : Chromosome
{
    TChromosome Chromosome { get; }
    double Fitness // TODO: DO away with chromosome having fitness
    {
        get => Chromosome.Fitness;
        set => Chromosome.Fitness = value;
    }

    IGeneticAgent<TChromosome> CrossOver(IGeneticAgent<TChromosome> other, double crossoverRate = 0.5);
    IGeneticAgent<TChromosome> Mutate(double mutationRate);
    IGeneticAgent<TChromosome> Clone();
    public string GetStableHash() => Chromosome.GetStableHash();
}
