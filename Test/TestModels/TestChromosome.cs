using SolvitaireCore;
using SolvitaireGenetics;

namespace Test;

public class TestChromosome : Chromosome
{
    public TestChromosome(Random random) : base(random) { }

    public TestChromosome() : this(Random.Shared) { }
}