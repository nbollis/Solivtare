using SolvitaireCore;
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

        var mutatedChromosome = Chromosome.Mutate(chromosome, 1.0);

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

        var child = Chromosome.Crossover(parent1, parent2, 1.0);

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

        var averageChromosome = Chromosome.GetAverageChromosome(new List<TestChromosome> { chromosome1, chromosome2 });

        Assert.That(averageChromosome.MutableStatsByName["stat1"], Is.EqualTo(2.0));
        Assert.That(averageChromosome.MutableStatsByName["stat2"], Is.EqualTo(3.0));
    }

    [Test]
    public void StableHashesAreUnique_Quadratic()
    {
        int count = 1000;
        QuadraticChromosome[] quadratic = new QuadraticChromosome[count];
        for (int i = 0; i < count; i++)
        {
            quadratic[i] = Chromosome.CreateRandom<QuadraticChromosome>(new Random());
        }

        var distinctChromosomes = quadratic.Distinct().ToList();
        Assert.That(quadratic.Count, Is.EqualTo(distinctChromosomes.Count), "Duplicate Detected.");

        var temp = quadratic.Select(p => string.Join(',', p.MutableStatsByName.Values)).ToList();
        var distinctTemp = temp.Distinct().ToList();
        Assert.That(temp.Count, Is.EqualTo(distinctTemp.Count), "Duplicate Detected.");

        var hashCodes = quadratic.Select(q => q.GetStableHash()).ToList();
        var distinctHashCodes = hashCodes.Distinct().ToList();
        Assert.That(hashCodes.Count, Is.EqualTo(distinctHashCodes.Count), "Hash code collision detected.");

        HashSet<string> hashSet = new ();
        foreach (var q in quadratic)
        {
            Assert.That(hashSet.Add(q.GetStableHash()), Is.True, "Hash code collision detected.");
        }
    }

    [Test]
    public void StableHashesAreUnique_Solitaire()
    {
        int count = 1000;
        SolitaireChromosome[] testChromosomes = new SolitaireChromosome[count];
        for (int i = 0; i < count; i++)
        {
            testChromosomes[i] = Chromosome.CreateRandom<SolitaireChromosome>(new Random());
        }

        var distinctChromosomes = testChromosomes.Distinct().ToList();
        Assert.That(testChromosomes.Count, Is.EqualTo(distinctChromosomes.Count), "Duplicate Detected.");

        var temp = testChromosomes.Select(p => string.Join(',', p.MutableStatsByName.Values)).ToList();
        var distinctTemp = temp.Distinct().ToList();
        Assert.That(temp.Count, Is.EqualTo(distinctTemp.Count), "Duplicate Detected.");

        var hashCodes = testChromosomes.Select(q => q.GetStableHash()).ToList();
        var distinctHashCodes = hashCodes.Distinct().ToList();
        Assert.That(hashCodes.Count, Is.EqualTo(distinctHashCodes.Count), "Hash code collision detected.");

        HashSet<string> hashSet = new();
        foreach (var q in testChromosomes)
        {
            Assert.That(hashSet.Add(q.GetStableHash()), Is.True, "Hash code collision detected.");
        }
    }

    [Test]
    public void EuclideanDistance_ShouldReturnCorrectDistance()
    {
        var random = new Random();
        var chromosome1 = new SolitaireChromosome(random);
        chromosome1.MutableStatsByName.Clear();
        chromosome1.MutableStatsByName["stat1"] = 1.0;
        chromosome1.MutableStatsByName["stat2"] = 2.0;

        var chromosome2 = new SolitaireChromosome(random);
        chromosome2.MutableStatsByName.Clear();
        chromosome2.MutableStatsByName["stat1"] = 4.0;
        chromosome2.MutableStatsByName["stat2"] = 6.0;

        var distance = Chromosome.EuclideanDistance(chromosome1, chromosome2);

        Assert.That(distance, Is.EqualTo(5.0).Within(1e-5));
    }

    [Test]
    public void CosineSimilarity_ShouldReturnCorrectSimilarity()
    {
        var random = new Random();
        var chromosome1 = new SolitaireChromosome(random);
        chromosome1.MutableStatsByName.Clear();
        chromosome1.MutableStatsByName["stat1"] = 1.0;
        chromosome1.MutableStatsByName["stat2"] = 0.0;

        var chromosome2 = new SolitaireChromosome(random);
        chromosome2.MutableStatsByName.Clear();
        chromosome2.MutableStatsByName["stat1"] = 1.0;
        chromosome2.MutableStatsByName["stat2"] = 0.0;

        var similarity = Chromosome.CosineSimilarity(chromosome1, chromosome2);

        Assert.That(similarity, Is.EqualTo(1.0).Within(1e-5));
    }

    [Test]
    public void NormalizedMAE_ShouldReturnCorrectError()
    {
        var random = new Random();
        var chromosome1 = new SolitaireChromosome(random);
        chromosome1.MutableStatsByName.Clear();
        chromosome1.MutableStatsByName["stat1"] = 1.0;
        chromosome1.MutableStatsByName["stat2"] = 2.0;

        var chromosome2 = new SolitaireChromosome(random);
        chromosome2.MutableStatsByName.Clear();
        chromosome2.MutableStatsByName["stat1"] = 3.0;
        chromosome2.MutableStatsByName["stat2"] = 4.0;

        var error = Chromosome.NormalizedMAE(chromosome1, chromosome2, -3, 3);

        Assert.That(error, Is.EqualTo(0.3333).Within(1e-4));
    }
}
