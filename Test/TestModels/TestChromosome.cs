using SolvitaireGenetics;

namespace Test;

internal class TestChromosome : Chromosome<TestChromosome>
{
    public TestChromosome(Random random) : base(random) { }

    public TestChromosome() : this(Random.Shared) { }

    public override TestChromosome Clone()
    {
        var clone = new TestChromosome(Random);
        foreach (var kvp in MutableStatsByName)
        {
            clone.MutableStatsByName[kvp.Key] = kvp.Value;
        }
        return clone;
    }
}