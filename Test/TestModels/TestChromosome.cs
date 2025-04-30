using SolvitaireGenetics;

namespace Test;

internal class TestChromosome : Chromosome
{
    public TestChromosome(Random random) : base(random) { }

    public TestChromosome() : this(Random.Shared) { }
}