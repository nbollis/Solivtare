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
            BestChromosome = new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat1", 1.0 } } },
            AverageChromosome = new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat2", 2.0 } } },
            StdChromosome = new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat3", 3.0 } } }
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
               Chromosome = new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat1", 1.0 } } }
           },
           new AgentLog
           {
               Generation = 1,
               Fitness = 80.0,
               GamesWon = 8,
               MovesMade = 40,
               GamesPlayed = 18,
               Chromosome = new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat2", 2.0 } } }
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
            PopulationSize = 10000,
            MutationRate = 0,
            OutputDirectory = OutputDirectory
        };
        var template = new QuadraticChromosome();
        template.Set(QuadraticChromosome.A, 1.0);
        template.Set(QuadraticChromosome.B, 2.0);
        template.Set(QuadraticChromosome.C, 3.0);
        template.Set(QuadraticChromosome.YIntercept, 4.0);
        template.Set(QuadraticChromosome.EvalFunction, 1.0);

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
            PopulationSize = 36,
            MutationRate = 0.1,
            TournamentSize = 4,
            OutputDirectory = OutputDirectory,
            CorrectA = -6,
            CorrectB = 5,
            CorrectC = 4,
            CorrectIntercept = 3
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        //var field = typeof(QuadraticRegressionGeneticAlgorithm).GetField("CrossOverRate", BindingFlags.NonPublic | BindingFlags.Instance);
        //Assert.That(field, Is.Not.Null, "Field 'CrossOverRate' not found.");
        //field.SetValue(algorithm, 0.0);

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
}