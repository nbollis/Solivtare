using NUnit.Framework;
using SolvitaireGenetics;

namespace Test.Selection;

[TestFixture]
public class SelectionStrategyFactoryTests
{
    private List<TestChromosome> GeneratePopulation(int size, Random random)
    {
        return Enumerable.Range(0, size)
            .Select(i => new TestChromosome() {Fitness = random.NextDouble() * 100 })
            .ToList();
    }

    [TestCase(SelectionStrategy.Tournament, 3, 5, TestName = "Tournament_ParentsGreaterThanPopulation")]
    [TestCase(SelectionStrategy.Tournament, 10, 5, TestName = "Tournament_ParentsLessThanPopulation")]
    [TestCase(SelectionStrategy.Tournament, 5, 5, TestName = "Tournament_ParentsEqualToPopulation")]
    [TestCase(SelectionStrategy.TopHalf, 3, 5, TestName = "TopHalf_ParentsGreaterThanPopulation")]
    [TestCase(SelectionStrategy.TopHalf, 10, 5, TestName = "TopHalf_ParentsLessThanPopulation")]
    [TestCase(SelectionStrategy.TopHalf, 5, 5, TestName = "TopHalf_ParentsEqualToPopulation")]
    public void Select_ShouldReturnCorrectNumberOfParents_WhenPopulationSizeVaries(SelectionStrategy strategy, int populationSize, int numberOfParents)
    {
        // Arrange  
        var random = new Random();
        var parameters = new GeneticAlgorithmParameters()
        {
            TournamentSize = 3
        };

        var selectionStrategy = SelectionStrategyFactory.Create<TestChromosome>(strategy);

        var population = GeneratePopulation(populationSize, random);

        // Act  
        var selectedParents = selectionStrategy.Select(population,numberOfParents, parameters, random);

        // Assert  
        Assert.That(selectedParents.Count, Is.EqualTo(numberOfParents),
            $"Failed for strategy {strategy} with PopulationSize={populationSize} and NumberOfParents={numberOfParents}");

    }

    [TestCase(SelectionStrategy.Tournament, 3, 5, TestName = "Tournament_ParentsGreaterThanPopulation")]
    [TestCase(SelectionStrategy.Tournament, 10, 5, TestName = "Tournament_ParentsLessThanPopulation")]
    [TestCase(SelectionStrategy.Tournament, 5, 5, TestName = "Tournament_ParentsEqualToPopulation")]
    [TestCase(SelectionStrategy.TopHalf, 3, 5, TestName = "TopHalf_ParentsGreaterThanPopulation")]
    [TestCase(SelectionStrategy.TopHalf, 10, 5, TestName = "TopHalf_ParentsLessThanPopulation")]
    [TestCase(SelectionStrategy.TopHalf, 5, 5, TestName = "TopHalf_ParentsEqualToPopulation")]
    public void Select_ShouldReturnCorrectNumberOfParents(SelectionStrategy strategy, int populationSize, int numberOfParents)
    {
        // Arrange  
        var random = new Random();
        var parameters = new GeneticAlgorithmParameters()
        {
            TournamentSize = 3
        };

        var selectionStrategy = SelectionStrategyFactory.Create<TestChromosome>(strategy);
        var population = GeneratePopulation(populationSize, random);

        // Act  
        var selectedParents = selectionStrategy.Select(population, numberOfParents, parameters, random);

        // Assert  
        Assert.That(selectedParents.Count, Is.EqualTo(numberOfParents),
            $"Failed for strategy {strategy} with PopulationSize={populationSize} and NumberOfParents={numberOfParents}");
    }
}
