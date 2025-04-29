using SolvitaireGenetics;

namespace Test.Genetics;

[TestFixture]
public class ChromosomeTests
{

    [Test]
    public void Mutate_ShouldChangeWeightsBasedOnMutationRate()
    {
        var random = new Random();
        var chromosome = new TestChromosome(random);
        chromosome.MutableStatsByName["stat1"] = 1.0;
        chromosome.MutableStatsByName["stat2"] = 2.0;

        var mutatedChromosome = Chromosome<TestChromosome>.Mutate(chromosome, 1.0);

        Assert.That(mutatedChromosome.MutableStatsByName["stat1"], Is.Not.EqualTo(1.0));
        Assert.That(mutatedChromosome.MutableStatsByName["stat2"], Is.Not.EqualTo(2.0));
    }

    [Test]
    public void Crossover_ShouldCombineWeightsFromBothParents()
    {
        var random = new Random();
        var parent1 = new TestChromosome(random);
        parent1.MutableStatsByName["stat1"] = 1.0;
        parent1.MutableStatsByName["stat2"] = 2.0;

        var parent2 = new TestChromosome(random);
        parent2.MutableStatsByName["stat1"] = 3.0;
        parent2.MutableStatsByName["stat2"] = 4.0;

        var child = Chromosome<TestChromosome>.Crossover(parent1, parent2, 1.0);

        Assert.That(child.MutableStatsByName["stat1"], Is.EqualTo(3.0));
        Assert.That(child.MutableStatsByName["stat2"], Is.EqualTo(4.0));
    }

    [Test]
    public void GetAverageChromosome_ShouldReturnAverageWeights()
    {
        var random = new Random();
        var chromosome1 = new TestChromosome(random);
        chromosome1.MutableStatsByName["stat1"] = 1.0;
        chromosome1.MutableStatsByName["stat2"] = 2.0;

        var chromosome2 = new TestChromosome(random);
        chromosome2.MutableStatsByName["stat1"] = 3.0;
        chromosome2.MutableStatsByName["stat2"] = 4.0;

        var averageChromosome = Chromosome<TestChromosome>.GetAverageChromosome(new List<TestChromosome> { chromosome1, chromosome2 });

        Assert.That(averageChromosome.MutableStatsByName["stat1"], Is.EqualTo(2.0));
        Assert.That(averageChromosome.MutableStatsByName["stat2"], Is.EqualTo(3.0));
    }
}
