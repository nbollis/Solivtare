using SolvitaireCore;

namespace Test;

public class TestChromosome : Chromosome
{
    public TestChromosome(Random random) : base(random) { }

    public TestChromosome() : this(Random.Shared) { }
}

public class TestGeneticAgent(TestChromosome chromie) : IGeneticAgent<TestChromosome>
{
    public TestChromosome Chromosome { get; } = chromie;
    public IGeneticAgent<TestChromosome> CrossOver(IGeneticAgent<TestChromosome> other, double crossoverRate = 0.5)
    {
        return new TestGeneticAgent(Chromosome.CrossOver(other.Chromosome, crossoverRate));
    }

    public IGeneticAgent<TestChromosome> Mutate(double mutationRate)
    {
        return new TestGeneticAgent(Chromosome.Mutate<TestChromosome>(mutationRate));
    }

    public IGeneticAgent<TestChromosome> Clone()
    {
        return new TestGeneticAgent(Chromosome.Clone<TestChromosome>());
    }
}