using NUnit.Framework;
using SolvitaireGenetics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MathNet.Numerics.Statistics;
using SolvitaireGenetics.IO;
using SolvitaireIO.Database.Models;

namespace Test.Genetics;

[TestFixture]
public class GeneticAlgorithmLoggerTests
{
    private const string OutputDirectory = "TestOutput";
    public static IEnumerable<LoggingType> LoggingTypSource => Enum.GetValues<LoggingType>();

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
            Thread.Sleep(500);
            Directory.Delete(OutputDirectory, true);
        }
    }

    public static GeneticAlgorithmLogger GetLogger(LoggingType type)
    {
        return type switch
        {
            LoggingType.Json => new GeneticAlgorithmLogger<TestChromosome>(OutputDirectory),
            LoggingType.Database => new DatabaseGeneticAlgorithmLogger(new(null)), // null for in-memory
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    [Test]
    [TestCaseSource(nameof(LoggingTypSource))]
    public void GetAllGenerationalLogs_ShouldReturnAllLogs(LoggingType loggingType)
    {
        // Arrange  
        var logger = GetLogger(loggingType);

        var expectedLogs = new List<GenerationLog>
        {
            new GenerationLog { Generation = 0, BestFitness = 1.0, AverageFitness = 0.8, StdFitness = 0.1 },
            new GenerationLog { Generation = 1, BestFitness = 0.9, AverageFitness = 0.7, StdFitness = 0.2 }
        };

        foreach (var log in expectedLogs)
        {
            logger.LogGenerationInfo(log);
        }

        // Act  
        var result = logger.ReadGenerationLogs();

        // Assert  
        Assert.That(result.Count, Is.EqualTo(expectedLogs.Count));
        Assert.That(result.Select(r => r.Generation), Is.EquivalentTo(expectedLogs.Select(e => e.Generation)));
        Assert.That(result.Select(r => r.BestFitness), Is.EquivalentTo(expectedLogs.Select(e => e.BestFitness)));
    }

    [Test]
    [TestCaseSource(nameof(LoggingTypSource))]
    public void GetAllChromosomesByGeneration_ShouldReturnCorrectMapping(LoggingType loggingType)
    {
        // Arrange  
        var logger = GetLogger(loggingType);
        var chromosome1 = new TestChromosome();
        var chromosome2 = new TestChromosome();

        logger.LogAgentDetail(new AgentLog
        {
            Generation = 0,
            Fitness = 1.0,
            GamesWon = 10,
            MovesMade = 20,
            GamesPlayed = 5,
            Chromosome = ChromosomeLog.FromChromosome(chromosome1)
        });

        logger.LogAgentDetail(new AgentLog
        {
            Generation = 1,
            Fitness = 0.9,
            GamesWon = 8,
            MovesMade = 18,
            GamesPlayed = 4,
            Chromosome = ChromosomeLog.FromChromosome(chromosome2)
        });

        // Act  
        var result = logger.ReadAllAgentLogs()
            .GroupBy(p => p.Generation)
            .ToDictionary(p => p.Key, p => p.ToList());

        // Assert  
        Assert.That(result.ContainsKey(0));
        Assert.That(result.ContainsKey(1));
        Assert.That(result[0].Count, Is.EqualTo(1));
        Assert.That(result[1].Count, Is.EqualTo(1));
        Assert.That(result[0].First().Chromosome.Chromosome, Is.EqualTo(chromosome1));
        Assert.That(result[1].First().Chromosome.Chromosome, Is.EqualTo(chromosome2));
    }

    [Test]
    [TestCaseSource(nameof(LoggingTypSource))]
    public void GeneticAlgorithm_RunEvolutionInStages_ShouldReportCorrectAgentCount(LoggingType loggingType)
    {
        // Arrange  
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 4,
            MutationRate = 0.25,
            TournamentSize = 4,
            OutputDirectory = loggingType == LoggingType.Database ? null : OutputDirectory,
            LoggingType = loggingType
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var logger = algorithm.Logger;

        // Run a single round of evolution at a time
        Dictionary<int, List<QuadraticChromosome>> populationDictionary = new();
        algorithm.InitializePopulation();
        populationDictionary.Add(0, algorithm.Population.Select(p => p.Chromosome).ToList());
        for (int i = 1; i < 11; i++)
        {
            algorithm.RunEvolution(1);

            // Algorithm keeps consistent population size
            populationDictionary.Add(i, algorithm.Population.Select(p => p.Chromosome).ToList());
            Assert.That(populationDictionary[i].Count, Is.EqualTo(parameters.PopulationSize));

            // Generation was logged
            var generationLog = logger.ReadGenerationLogs();
            Assert.That(generationLog.Count, Is.EqualTo(i + 1)); // +1 because we start from 0
            Assert.That(generationLog.Last().Generation, Is.EqualTo(i));
            Assert.That(generationLog.Last().BestFitness,
                Is.EqualTo(populationDictionary[i][0].Fitness).Within(0.000000001));
            Assert.That(generationLog.Last().AverageFitness,
                Is.EqualTo(populationDictionary[i].Average(c => c.Fitness)));
            Assert.That(generationLog.Last().StdFitness,
                Is.EqualTo(populationDictionary[i].Select(c => c.Fitness).StandardDeviation()));

            // Agents were logged
            var agents = logger.ReadAllAgentLogs()
                .GroupBy(p => p.Generation)
                .ToDictionary(p => p.Key, p => p.ToList());

            Assert.That(agents.Count, Is.EqualTo(i + 1)); // +1 because we start from 0
            Assert.That(agents[i].Sum(p => p.Count), Is.EqualTo(parameters.PopulationSize));
            Assert.That(agents[i].All(a => a.Generation == i));

            var agentsFromThisGen = agents[i];
            var populationFromThisGen = populationDictionary[i];

            // All chromosomes in the population are represented in the log
            foreach (var agent in agentsFromThisGen)
            {
                var matchingChromosome = populationFromThisGen.FirstOrDefault(c => c.Equals(agent.Chromosome.Chromosome));
                if (matchingChromosome == null)
                {
                    Console.WriteLine($"Mismatch found for Chromosome: {agent.Chromosome}");
                    Console.WriteLine($"Agent Chromosome Fitness: {agent.Chromosome.Fitness}");
                    Console.WriteLine($"Population Chromosomes: {string.Join(", ", populationFromThisGen.Select(c => c.Fitness))}");
                }

                Assert.That(matchingChromosome, Is.Not.Null, $"Chromosome not found: {agent.Chromosome}");
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(LoggingTypSource))]
    public void GeneticAlgorithmLogger_CreateTsvSummariesHasCorrectChromosomes(LoggingType loggingType)
    {
        // Arrange  
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 4,
            MutationRate = 0.25,
            TournamentSize = 4,
            Generations = 100,
            OutputDirectory = loggingType == LoggingType.Database ? null : OutputDirectory,
            LoggingType = loggingType
        };

        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var logger = algorithm.Logger;

        List<GenerationLog> generations = new();
        algorithm.GenerationCompleted += (a, l) =>
        {
            // Log the generation info
            generations.Add(l);
        };

        // Run the evolution
        algorithm.RunEvolution(100);
        logger.CreateTsvSummaries(OutputDirectory);

        // Act
        var averageTsv = AgentLogTabFile.ReadFromFile(Path.Combine(OutputDirectory, "AverageChromosome.tsv"));
        var bestTsv = AgentLogTabFile.ReadFromFile(Path.Combine(OutputDirectory, "BestChromosome.tsv"));

        // Assert
        Assert.That(averageTsv.Count, Is.EqualTo(generations.Count));
        Assert.That(bestTsv.Count, Is.EqualTo(generations.Count));

        for (int i = 0; i < generations.Count; i++)
        {
            var averageChromosome = averageTsv[i].Chromosome;
            var bestChromosome = bestTsv[i].Chromosome;

            Assert.That(averageChromosome.Chromosome, Is.EqualTo(generations[i].AverageChromosome.Chromosome));
            Assert.That(bestChromosome.Chromosome, Is.EqualTo(generations[i].BestChromosome.Chromosome));
        }

    }
}