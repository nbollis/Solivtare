using SolvitaireGenetics;

namespace Test.Genetics;

[TestFixture]
public class ReproductionStrategyTests
{
    [TestCase(ReproductionStrategy.Sexual)]
    [TestCase(ReproductionStrategy.Asexual)]
    public void Reproduce_ShouldMatchTargetPopulationCount_WhenParentsAreLessThanTargetPopulation(ReproductionStrategy strategy)
    {
        // Arrange  
        var parents = new List<TestGeneticAgent>
        {
            new TestGeneticAgent(new()),
            new TestGeneticAgent(new())
        };
        var targetPopulation = 5;
        var crossoverRate = 0.5;
        var mutationRate = 0.1;
        var random = new Random();
        var reproductionStrategy = ReproductionStrategyFactory.Create<TestGeneticAgent, TestChromosome>(strategy);

        // Act  
        var offspring = reproductionStrategy.Reproduce(parents, targetPopulation, crossoverRate, mutationRate, random);

        // Assert  
        Assert.That(offspring.Count, Is.EqualTo(targetPopulation));
    }

    [TestCase(ReproductionStrategy.Sexual)]
    [TestCase(ReproductionStrategy.Asexual)]
    public void Reproduce_ShouldMatchTargetPopulationCount_WhenParentsAreEqualToTargetPopulation(
        ReproductionStrategy strategy)
    {
        // Arrange  
        var parents = new List<TestGeneticAgent>
        {
            new TestGeneticAgent(new()),
            new TestGeneticAgent(new()),
            new TestGeneticAgent(new()),
            new TestGeneticAgent(new())
        };
        var targetPopulation = 4;
        var crossoverRate = 0.5;
        var mutationRate = 0.1;
        var random = new Random();
        var reproductionStrategy = ReproductionStrategyFactory.Create<TestGeneticAgent, TestChromosome>(strategy);

        // Act  
        var offspring = reproductionStrategy.Reproduce(parents, targetPopulation, crossoverRate, mutationRate, random);

        // Assert  
        Assert.That(offspring.Count, Is.EqualTo(targetPopulation));
    }

    [TestCase(ReproductionStrategy.Sexual)]
    [TestCase(ReproductionStrategy.Asexual)]
    public void Reproduce_ShouldMatchTargetPopulationCount_WhenParentsAreGreaterThanTargetPopulation(ReproductionStrategy strategy)
    {
        // Arrange  
        var parents = new List<TestGeneticAgent>
        {
            new TestGeneticAgent(new()),
            new TestGeneticAgent(new()),
            new TestGeneticAgent(new()),
            new TestGeneticAgent(new())
        };
        var targetPopulation = 3;
        var crossoverRate = 0.5;
        var mutationRate = 0.1;
        var random = new Random();
        var reproductionStrategy = ReproductionStrategyFactory.Create<TestGeneticAgent, TestChromosome>(strategy);

        // Act  
        var offspring = reproductionStrategy.Reproduce(parents, targetPopulation, crossoverRate, mutationRate, random);

        // Assert  
        Assert.That(offspring.Count, Is.EqualTo(targetPopulation));
    }
}
