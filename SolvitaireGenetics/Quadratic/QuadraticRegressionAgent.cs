using SolvitaireCore;

namespace SolvitaireGenetics;

public class QuadraticRegressionAgent : IGeneticAgent<QuadraticChromosome>
{
    public double Fitness
    {
        get => Chromosome.Fitness;
        set => Chromosome.Fitness = value;
    }

    public QuadraticChromosome Chromosome { get; }
    public QuadraticRegressionAgent(QuadraticChromosome chromosome) { Chromosome = chromosome; }
    public IGeneticAgent<QuadraticChromosome> CrossOver(IGeneticAgent<QuadraticChromosome> other, double crossoverRate = 0.5)
        => new QuadraticRegressionAgent(Chromosome.CrossOver(other.Chromosome, crossoverRate));

    public IGeneticAgent<QuadraticChromosome> Mutate(double mutationRate)
        => new QuadraticRegressionAgent(Chromosome.Mutate<QuadraticChromosome>(mutationRate));

    public IGeneticAgent<QuadraticChromosome> Clone()
        => new QuadraticRegressionAgent(Chromosome.Clone<QuadraticChromosome>()); 
}