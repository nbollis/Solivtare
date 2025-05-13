using System.Reflection;
using System.Text.Json;
using SolvitaireCore;
using SolvitaireGenetics;
using SolvitaireIO.Database.Models;

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
            Thread.Sleep(200); // Sleep to ensure the population is initialized and logged
            Directory.Delete(OutputDirectory, true);
        }
    }

    [Test]
    public void LoadLastGeneration_ShouldRestoreLastGenerationChromosomes()
    {
        // Arrange  
        var logger = new GeneticAlgorithmLogger<TestChromosome>(OutputDirectory);

        var generationLog = new GenerationLog
        {
            Generation = 1,
            BestFitness = 100.0,
            AverageFitness = 50.0,
            BestChromosome = ChromosomeLog.FromChromosome(new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat1", 1.0 } } }),
            AverageChromosome = ChromosomeLog.FromChromosome(new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat2", 2.0 } } }),
            StdChromosome = ChromosomeLog.FromChromosome(new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat3", 3.0 } } })
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
               Chromosome = ChromosomeLog.FromChromosome(new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat1", 1.0 } } })
           },
           new AgentLog
           {
               Generation = 1,
               Fitness = 80.0,
               GamesWon = 8,
               MovesMade = 40,
               GamesPlayed = 18,
               Chromosome = ChromosomeLog.FromChromosome(new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat2", 2.0 } } })
           }
       };

        File.WriteAllText(Path.Combine(OutputDirectory, "GenerationalLog.json"), JsonSerializer.Serialize(new List<GenerationLog> { generationLog }));
        File.WriteAllText(Path.Combine(OutputDirectory, "AgentLog.json"), JsonSerializer.Serialize(agentLogs));

        // Act  
        var lastGenerationChromosomes = logger.LoadLastGeneration(out _).Select(p => p.Chromosome).ToList();

        // Assert  
        Assert.That(lastGenerationChromosomes.Count, Is.EqualTo(2));
        Assert.That(lastGenerationChromosomes[0].Chromosome.MutableStatsByName["Stat1"], Is.EqualTo(1.0));
        Assert.That(lastGenerationChromosomes[1].Chromosome.MutableStatsByName["Stat2"], Is.EqualTo(2.0));
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
    public void GeneticAlgorithm_RunEvolutionMultipleTimes_ShouldAccumulateGenerations()
    {
        // Arrange  
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 15,
            MutationRate = 0.2,
            TournamentSize = 4,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Act - Run the first 5 generations  
        algorithm.RunEvolution(5);
        var firstRunGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the first run completed correctly  
        Assert.That(firstRunGeneration, Is.EqualTo(5));

        // Act - Run another 5 generations  
        algorithm.RunEvolution(5);
        var secondRunGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the total generations accumulated correctly  
        Assert.That(secondRunGeneration, Is.EqualTo(10));
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
            TournamentSize = 10,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var population = algorithm.InitializePopulation();
        var avgChromosome = population.Average(c => c.Fitness);

        // Act
        // Use reflection to access the protected TournamentSelection method
        var tournamentSelectionMethod = typeof(GeneticAlgorithm<QuadraticChromosome, QuadraticGeneticAlgorithmParameters>)
            .GetMethod("TournamentSelection", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.That(tournamentSelectionMethod, Is.Not.Null, "TournamentSelection method not found.");

        var selectedChromosome = (List<QuadraticChromosome>)tournamentSelectionMethod.Invoke(algorithm, new object[] { population, 1 });

        // Assert
        Assert.That(selectedChromosome, Is.Not.Null);
        Assert.That(population.Contains(selectedChromosome[0]));
        Assert.That(selectedChromosome[0].Fitness, Is.GreaterThanOrEqualTo(avgChromosome));
    }

    [Test]
    public void GeneticAlgorithm_TournamentSelection_ShouldReturnBestChromosome()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 10,
            TournamentSize = 10,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var population = algorithm.InitializePopulation();
        var bestChromosome = population.OrderByDescending(c => c.Fitness).First();

        // Act
        // Use reflection to access the protected TournamentSelection method
        var tournamentSelectionMethod = typeof(GeneticAlgorithm<QuadraticChromosome, QuadraticGeneticAlgorithmParameters>)
            .GetMethod("TournamentSelection", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.That(tournamentSelectionMethod, Is.Not.Null, "TournamentSelection method not found.");

        var selectedChromosome = (List<QuadraticChromosome>)tournamentSelectionMethod.Invoke(algorithm, new object[] { population, 1 });

        // Assert
        Assert.That(selectedChromosome[0].Fitness, Is.EqualTo(bestChromosome.Fitness).Within(0.0001));
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
            { QuadraticChromosome.EvalFunction, 1 }
        };

        var lastGeneration = new List<QuadraticChromosome>()
        {
            new QuadraticChromosome { Fitness = 1.0, MutableStatsByName  = modelStats},
            new QuadraticChromosome { Fitness = 2.0, MutableStatsByName  = modelStats }
        };

        var logger = new GeneticAlgorithmLogger<QuadraticChromosome>(OutputDirectory);
        logger.LogGenerationInfo(0, 2, 1.5, 0.5, lastGeneration[1], Chromosome.GetAverageChromosome(lastGeneration), Chromosome.GetStandardDeviationChromosome(lastGeneration) );
        foreach (var chromosome in lastGeneration)
        {
            logger.AccumulateAgentLog(0, chromosome, chromosome.Fitness, 1, 1, 1);
        }
        logger.FlushAgentLogs(0, lastGeneration);


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
        template.SetWeight(QuadraticChromosome.A, 1.0);
        template.SetWeight(QuadraticChromosome.B, 2.0);
        template.SetWeight(QuadraticChromosome.C, 3.0);
        template.SetWeight(QuadraticChromosome.YIntercept, 4.0);
        template.SetWeight(QuadraticChromosome.EvalFunction, 1.0);
        parameters.TemplateChromosome = template;

        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var field = typeof(QuadraticRegressionGeneticAlgorithm).GetField("CrossOverRate", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null, "Field 'CrossOverRate' not found.");
        field.SetValue(algorithm, 0.0);

        // Act
        var population = algorithm.InitializePopulation();

        // Assert
        Assert.That(population.Count, Is.EqualTo(parameters.PopulationSize));
        var mutatedTemplateCount = population.Count(c => Math.Abs(c.GetWeight(QuadraticChromosome.A) - 1.0) < 0.000001);

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
            PopulationSize = 48,
            MutationRate = 0.1,
            TournamentSize = 4,
            OutputDirectory = OutputDirectory,
            CorrectA = -6,
            CorrectB = 5,
            CorrectC = 4,
            CorrectIntercept = 3
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var field = typeof(QuadraticRegressionGeneticAlgorithm).GetField("CrossOverRate", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null, "Field 'CrossOverRate' not found.");
        field.SetValue(algorithm, 0.0);

        // Act - Run the first 3 generations  
        var firstFitness = algorithm.RunEvolution(1).Fitness;
        var firstStageGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the first stage completed correctly  
        Assert.That(firstStageGeneration, Is.EqualTo(1));

        // Act - Run 2 more generations  
        var secondFitness = algorithm.RunEvolution(1).Fitness;
        var secondGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the total generations completed correctly  
        Assert.That(secondGeneration, Is.EqualTo(2));
        Assert.That(firstFitness, Is.Not.EqualTo(secondFitness));
        Assert.That(firstFitness, Is.LessThan(secondFitness));

        // Act - Run 3 more generations  
        var thirdFitness = algorithm.RunEvolution(1).Fitness;
        var finalGeneration = algorithm.CurrentGeneration;

        // Assert - Ensure the total generations completed correctly  
        Assert.That(finalGeneration, Is.EqualTo(3));
        Assert.That(firstFitness, Is.Not.EqualTo(thirdFitness));
        Assert.That(firstFitness, Is.LessThan(thirdFitness));
        Assert.That(secondFitness, Is.Not.EqualTo(thirdFitness));
        Assert.That(secondFitness, Is.LessThan(thirdFitness));
    }

    [Test]
    public void TournamentSelection_ShouldDistributeChromosomesFairly()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 10,
            TournamentSize = 1,
            OutputDirectory = null
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var population = algorithm.InitializePopulation();

        // Act
        var tournamentSelectionMethod = typeof(GeneticAlgorithm<QuadraticChromosome, QuadraticGeneticAlgorithmParameters>)
            .GetMethod("TournamentSelection", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.That(tournamentSelectionMethod, Is.Not.Null, "TournamentSelection method not found.");

        var selectedParents = (List<QuadraticChromosome>)tournamentSelectionMethod.Invoke(algorithm, new object[] { population, 30 });


        // Assert
        var selectionCounts = population.ToDictionary(c => c, c => selectedParents.Count(p => p == c));
        foreach (var count in selectionCounts.Values)
        {
            Assert.That(count, Is.GreaterThan(0), "Every chromosome should be selected at least once.");
        }
    }

    [Test]
    public void GeneticAlgorithm_ThanosSnapTriggered_ShouldHalvePopulation()
    {
        // Arrange
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 10,
            Generations = 2,
            MutationRate = 0.1,
            TournamentSize = 3,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);

        // Initialize population and run one generation
        algorithm.InitializePopulation();
        var initialPopulationCount = algorithm.Population.Count;

        // Act: Set ThanosSnapTriggered and run one more generation
        algorithm.ThanosSnapTriggered = true;
        algorithm.RunEvolution(1);

        // Assert: Population should be halved (rounded down)
        var expectedCount = initialPopulationCount / 2;
        Assert.That(algorithm.Population.Count, Is.EqualTo(expectedCount));
        Assert.That(algorithm.ThanosSnapTriggered, Is.False, "ThanosSnapTriggered should reset to false after snap.");
    }
}