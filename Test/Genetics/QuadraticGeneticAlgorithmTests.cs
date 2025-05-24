using SolvitaireCore;
using SolvitaireGenetics;

namespace Test.Genetics;

[TestFixture]
public class QuadraticGeneticAlgorithmTests
{
    [Test]
    public void QuadraticChromosome_Constructor_ShouldInitializeWeights()
    {
        // Arrange & Act
        var chromosome = new QuadraticChromosome();

        // Assert
        Assert.That(chromosome.MutableStatsByName.ContainsKey(QuadraticChromosome.A));
        Assert.That(chromosome.MutableStatsByName.ContainsKey(QuadraticChromosome.B));
        Assert.That(chromosome.MutableStatsByName.ContainsKey(QuadraticChromosome.C));
        Assert.That(chromosome.MutableStatsByName.ContainsKey(QuadraticChromosome.YIntercept));
    }

    [Test]
    public void QuadraticChromosome_GetAndSet_ShouldWorkCorrectly()
    {
        // Arrange
        var chromosome = new QuadraticChromosome();
        var newValue = 2.5;

        // Act
        chromosome.SetWeight(QuadraticChromosome.A, newValue);
        var result = chromosome.GetWeight(QuadraticChromosome.A);

        // Assert
        Assert.That(result, Is.EqualTo(newValue));
    }
    [Test]
    public void QuadraticRegressionGeneticAlgorithm_EvaluateFitness_ShouldReturnCorrectRange()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            CorrectA = 1.0,
            CorrectB = 2.0,
            CorrectC = 3.0,
            CorrectIntercept = 4.0
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var chromosome = new QuadraticChromosome();
        chromosome.SetWeight(QuadraticChromosome.A, 1.0);
        chromosome.SetWeight(QuadraticChromosome.B, 2.0);
        chromosome.SetWeight(QuadraticChromosome.C, 3.0);
        chromosome.SetWeight(QuadraticChromosome.YIntercept, 4.0);
        var agent = new QuadraticRegressionAgent(chromosome);

        // Act
        var fitness = algorithm.EvaluateFitness(agent);

        // Assert
        Assert.That(fitness, Is.GreaterThanOrEqualTo(-1.0));
        Assert.That(fitness, Is.LessThanOrEqualTo(1.0));
    }

    [Test]
    public void QuadraticRegressionGeneticAlgorithm_Constructor_ShouldInitializeCorrectLine()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            CorrectA = 1.0,
            CorrectB = 2.0,
            CorrectC = 3.0,
            CorrectIntercept = 4.0,
            OutputDirectory = null
        };

        // Act
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Assert
        Assert.That(algorithm.CorrectLine.Count, Is.EqualTo(10000)); // From -10 to 10
    }


    [Test]
    public void GeneticAlgorithm_RunEvolutionInStages_ShouldCompleteAllGenerations()
    {
        // Arrange  
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 16,
            MutationRate = 0.2,
            TournamentSize = 2
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Act - Run the first 3 generations  
        var firstFitness = algorithm.RunEvolution(3).Fitness;
        var firstStageGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the first stage completed correctly  
        Assert.That(firstStageGeneration, Is.EqualTo(3));

        // Act - Run 2 more generations  
        var secondFitness = algorithm.RunEvolution(3).Fitness;
        var finalGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the total generations completed correctly  
        Assert.That(finalGeneration, Is.EqualTo(6));
        Assert.That(firstFitness, Is.Not.EqualTo(secondFitness));
    }

    [Test]
    public void QuadraticRegressionGeneticAlgorithm_EvaluateFitness_ShouldReturnPerfectScoreForCorrectChromosome()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            CorrectA = 1.0,
            CorrectB = 2.0,
            CorrectC = 3.0,
            CorrectIntercept = 4.0
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Create a chromosome with the exact correct parameters
        var correctChromosome = new QuadraticChromosome();
        correctChromosome.SetWeight(QuadraticChromosome.A, parameters.CorrectA);
        correctChromosome.SetWeight(QuadraticChromosome.B, parameters.CorrectB);
        correctChromosome.SetWeight(QuadraticChromosome.C, parameters.CorrectC);
        correctChromosome.SetWeight(QuadraticChromosome.YIntercept, parameters.CorrectIntercept);
        var agent = new QuadraticRegressionAgent(correctChromosome);

        // Act
        var fitness = algorithm.EvaluateFitness(agent);

        // Assert
        Assert.That(fitness, Is.EqualTo(1.0), "The fitness of the correct chromosome should be perfect (1.0).");
    }
}