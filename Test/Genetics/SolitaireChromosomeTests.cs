using SolvitaireCore;

namespace Test.Genetics;

[TestFixture]
public class SolitaireChromosomeTests
{
    private Random _random;

    [SetUp]
    public void SetUp()
    {
        _random = new Random();
    }

    [Test]
    public void Constructor_ShouldInitializeWeights()
    {
        // Act  
        var chromosome = new SolitaireChromosome(_random);

        // Assert  
        Assert.That(chromosome.MutableStatsByName, Is.Not.Empty);
        Assert.That(chromosome.MutableStatsByName.Keys, Contains.Item(SolitaireChromosome.LegalMoveWeightName));
        Assert.That(chromosome.MutableStatsByName.Keys, Contains.Item(SolitaireChromosome.FoundationWeightName));
    }

    [Test]
    public void Clone_ShouldCreateExactCopy()
    {
        // Arrange  
        var chromosome = new SolitaireChromosome(_random);

        // Act  
        var clone = chromosome.Clone();

        // Assert  
        Assert.That(clone, Is.Not.SameAs(chromosome));
        Assert.That(clone.MutableStatsByName, Is.EqualTo(chromosome.MutableStatsByName));
    }

    [Test]
    public void GetWeight_ExistingWeight_ShouldReturnCorrectValue()
    {
        // Arrange  
        var chromosome = new SolitaireChromosome(_random);
        var weightName = SolitaireChromosome.LegalMoveWeightName;
        var expectedValue = chromosome.MutableStatsByName[weightName];

        // Act  
        var value = chromosome.GetWeight(weightName);

        // Assert  
        Assert.That(value, Is.EqualTo(expectedValue));
    }

    [Test]
    public void GetWeight_NonExistingWeight_ShouldReturnZero()
    {
        // Arrange  
        var chromosome = new SolitaireChromosome(_random);

        // Act  
        var value = chromosome.GetWeight("NonExistingWeight");

        // Assert  
        Assert.That(value, Is.EqualTo(0));
    }

    [Test]
    public void SetWeight_ShouldUpdateWeightValue()
    {
        // Arrange  
        var chromosome = new SolitaireChromosome(_random);
        var weightName = SolitaireChromosome.LegalMoveWeightName;
        var newValue = 1.5;

        // Act  
        chromosome.SetWeight(weightName, newValue);

        // Assert  
        Assert.That(chromosome.MutableStatsByName[weightName], Is.EqualTo(newValue));
    }
}
