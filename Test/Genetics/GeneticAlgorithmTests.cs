using System.Reflection;
using System.Text.Json;
using SolvitaireGenetics;

namespace Test.Genetics;

[TestFixture]
public class GeneticAlgorithmTests
{
    private const string OutputDirectory = "TestOutput";

    [SetUp]
    public void SetUp()
    {
        if (Directory.Exists(OutputDirectory))
        {
            Directory.Delete(OutputDirectory, true);
        }
        Directory.CreateDirectory(OutputDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(OutputDirectory))
        {
            Directory.Delete(OutputDirectory, true);
        }
    }

    [Test]
    public void LoadLastGeneration_ShouldRestoreLastGenerationChromosomes()
    {
        // Arrange  
        var logger = new GeneticAlgorithmLogger<TestChromosome>(OutputDirectory);

        var generationLog = new GenerationLogDto
        {
            Generation = 1,
            BestFitness = 100.0,
            AverageFitness = 50.0,
            BestChromosome = new ChromosomeDto { Weights = new Dictionary<string, double> { { "Stat1", 1.0 } } },
            AverageChromosome = new ChromosomeDto { Weights = new Dictionary<string, double> { { "Stat2", 2.0 } } },
            StdChromosome = new ChromosomeDto { Weights = new Dictionary<string, double> { { "Stat3", 3.0 } } }
        };

        var agentLogs = new List<AgentLog>
       {
           new AgentLog
           {
               Generation = 1,
               Fitness = 100.0,
               GamesWon = 10,
               MovesMade = 50,
               GamesPlayed = 20,
               Chromosome = new ChromosomeDto { Weights = new Dictionary<string, double> { { "Stat1", 1.0 } } }
           },
           new AgentLog
           {
               Generation = 1,
               Fitness = 80.0,
               GamesWon = 8,
               MovesMade = 40,
               GamesPlayed = 18,
               Chromosome = new ChromosomeDto { Weights = new Dictionary<string, double> { { "Stat2", 2.0 } } }
           }
       };

        File.WriteAllText(Path.Combine(OutputDirectory, "GenerationalLog.json"), JsonSerializer.Serialize(new List<GenerationLogDto> { generationLog }));
        File.WriteAllText(Path.Combine(OutputDirectory, "AgentLog.json"), JsonSerializer.Serialize(agentLogs));

        // Act  
        var lastGenerationChromosomes = logger.LoadLastGeneration(out _);

        // Assert  
        Assert.That(lastGenerationChromosomes.Count, Is.EqualTo(2));
        Assert.That(lastGenerationChromosomes[0].MutableStatsByName["Stat1"], Is.EqualTo(1.0));
        Assert.That(lastGenerationChromosomes[1].MutableStatsByName["Stat2"], Is.EqualTo(2.0));
    }

    [Test]
    public void GeneticAlgorithm_RunEvolution_ShouldCompleteGenerations()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 10,
            Generations = 5,
            MutationRate = 0.1,
            TournamentSize = 3,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Act
        algorithm.RunEvolution(parameters.Generations);

        // Assert
        Assert.That(algorithm.CurrentGeneration, Is.EqualTo(parameters.Generations));
    }

    [Test]
    public void GeneticAlgorithm_InitializePopulation_ShouldCreateCorrectSize()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 10,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Act
        var population = algorithm.InitializePopulation();

        // Assert
        Assert.That(population.Count, Is.EqualTo(parameters.PopulationSize));
    }

    [Test]
    public void GeneticAlgorithm_TournamentSelection_ShouldReturnValidChromosome()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 10,
            TournamentSize = 3,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var population = algorithm.InitializePopulation();
        var avgChromosome = population.Average(c => c.Fitness);

        // Act
        var selectedChromosome = algorithm.TournamentSelection(population);

        // Assert
        Assert.That(selectedChromosome, Is.Not.Null);
        Assert.That(population.Contains(selectedChromosome));
        Assert.That(selectedChromosome.Fitness, Is.GreaterThanOrEqualTo(avgChromosome));
    }

    [Test]
    public void InitializePopulation_ShouldUseLastGenerationIfAvailable()
    {
        // Arrange
        Dictionary<string, double> modelStats = new Dictionary<string, double>()
        {
            { QuadraticChromosome.A, 1 }, 
            { QuadraticChromosome.B, 1 }, 
            { QuadraticChromosome.C, 1 }, 
            { QuadraticChromosome.YIntercept, 1 }, 
        };

        var lastGeneration = new List<QuadraticChromosome>()
        {
            new QuadraticChromosome { Fitness = 1.0, MutableStatsByName  = modelStats},
            new QuadraticChromosome { Fitness = 2.0, MutableStatsByName  = modelStats }
        };

        var logger = new GeneticAlgorithmLogger<QuadraticChromosome>(OutputDirectory);
        foreach (var chromosome in lastGeneration)
        {
            logger.AccumulateAgentLog(0, chromosome, chromosome.Fitness, 1, 1, 1);
        }
        logger.FlushAgentLogs();


        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = lastGeneration.Count,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Act
        var population = algorithm.InitializePopulation();

        // Assert
        Assert.That(population.Count, Is.EqualTo(parameters.PopulationSize));
        for (var index = 0; index < lastGeneration.Count; index++)
        {
            var og = lastGeneration[index];
            var loaded = population[index];
            Assert.That(og.Fitness, Is.EqualTo(loaded.Fitness));
            Assert.That(og.MutableStatsByName[QuadraticChromosome.A], Is.EqualTo(loaded.MutableStatsByName[QuadraticChromosome.A]));
            Assert.That(og.MutableStatsByName[QuadraticChromosome.B], Is.EqualTo(loaded.MutableStatsByName[QuadraticChromosome.B]));
            Assert.That(og.MutableStatsByName[QuadraticChromosome.C], Is.EqualTo(loaded.MutableStatsByName[QuadraticChromosome.C]));
            Assert.That(og.MutableStatsByName[QuadraticChromosome.YIntercept], Is.EqualTo(loaded.MutableStatsByName[QuadraticChromosome.YIntercept]));
        }
    }

    [Test]
    public void InitializePopulation_ShouldUseChromosomeTemplateIfAvailable()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 1000,
            MutationRate = 0,
            OutputDirectory = OutputDirectory
        };
        var template = new QuadraticChromosome();
        template.Set(QuadraticChromosome.A, 1.0);
        template.Set(QuadraticChromosome.B, 2.0);
        template.Set(QuadraticChromosome.C, 3.0);
        template.Set(QuadraticChromosome.YIntercept, 4.0);

        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters, template);
        var field = typeof(QuadraticRegressionGeneticAlgorithm).GetField("CrossOverRate", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null, "Field 'CrossOverRate' not found.");
        field.SetValue(algorithm, 0.0);

        // Act
        var population = algorithm.InitializePopulation();

        // Assert
        Assert.That(population.Count, Is.EqualTo(parameters.PopulationSize));
        var mutatedTemplateCount = population.Count(c => Math.Abs(c.Get(QuadraticChromosome.A) - 1.0) < 0.000001);

        int expectedCount = (parameters.PopulationSize / 10) + (int)(parameters.PopulationSize * 0.9 / 2);

        // 10% are copies of the template
        // The remaining have had a 50% chance to cross over. 
        Assert.That(mutatedTemplateCount, Is.EqualTo(expectedCount).Within(40)); 
    }

    [Test]
    public void InitializePopulation_ShouldCreateRandomPopulationIfNoLastGenerationOrTemplate()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 10,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Act
        var population = algorithm.InitializePopulation();

        // Assert
        Assert.That(population.Count, Is.EqualTo(parameters.PopulationSize));
        Assert.That(population.All(c => c is not null));
    }

    [Test]
    public void GeneticAlgorithm_RunEvolutionInStages_ShouldImprove()
    {
        // Arrange  
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 10,
            MutationRate = 0.01,
            TournamentSize = 8,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Act - Run the first 3 generations  
        var firstFitness = algorithm.RunEvolution(5).Fitness;
        var firstStageGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the first stage completed correctly  
        Assert.That(firstStageGeneration, Is.EqualTo(5));

        // Act - Run 2 more generations  
        var secondFitness = algorithm.RunEvolution(5).Fitness;
        var secondGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the total generations completed correctly  
        Assert.That(secondGeneration, Is.EqualTo(10));
        Assert.That(firstFitness, Is.Not.EqualTo(secondFitness));
        Assert.That(firstFitness, Is.LessThan(secondFitness));

        // Act - Run 3 more generations  
        var thirdFitness = algorithm.RunEvolution(5).Fitness;
        var finalGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the total generations completed correctly  
        Assert.That(finalGeneration, Is.EqualTo(15));
        Assert.That(firstFitness, Is.Not.EqualTo(thirdFitness));
        Assert.That(firstFitness, Is.LessThan(thirdFitness));
        Assert.That(secondFitness, Is.Not.EqualTo(thirdFitness));
        Assert.That(secondFitness, Is.LessThan(thirdFitness));
    }
}