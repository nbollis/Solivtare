using NUnit.Framework;
using SolvitaireGenetics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MathNet.Numerics.Statistics;

namespace Test.Genetics;

[TestFixture]
public class GeneticAlgorithmLoggerTests
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
    public void GetAllGenerationalLogs_ShouldReturnAllLogs()
    {
        // Arrange  
        var logger = new GeneticAlgorithmLogger<TestChromosome>("TestOutput");
        var expectedLogs = new List<GenerationLogDto>
        {
            new GenerationLogDto { Generation = 0, BestFitness = 1.0, AverageFitness = 0.8, StdFitness = 0.1 },
            new GenerationLogDto { Generation = 1, BestFitness = 0.9, AverageFitness = 0.7, StdFitness = 0.2 }
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
    public void GetAllChromosomesByGeneration_ShouldReturnCorrectMapping()
    {
        // Arrange  
        var logger = new GeneticAlgorithmLogger<TestChromosome>("TestOutput");
        var chromosome1 = new TestChromosome();
        var chromosome2 = new TestChromosome();

        logger.LogAgentDetail(0, chromosome1, 1.0, 10, 20, 5);
        logger.LogAgentDetail(1, chromosome2, 0.9, 8, 18, 4);

        // Act  
        var result = logger.ReadAllAgentLogs()
            .GroupBy(p => p.Generation)
            .ToDictionary(p => p.Key, p => p.ToList());

        // Assert  
        Assert.That(result.ContainsKey(0));
        Assert.That(result.ContainsKey(1));
        Assert.That(result[0].Count, Is.EqualTo(1));
        Assert.That(result[1].Count, Is.EqualTo(1));
        Assert.That(result[0].First().Chromosome, Is.EqualTo(chromosome1));
        Assert.That(result[1].First().Chromosome, Is.EqualTo(chromosome2));
    }

    [Test]
    public void GeneticAlgorithm_RunEvolutionInStages_ShouldReportCorrectAgentCount()
    {
        // Arrange  
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 4,
            MutationRate = 0.25,
            TournamentSize = 4,
            OutputDirectory = OutputDirectory
        };
        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var logger = algorithm.Logger;

        // Run a single round of evolution at a time
        Dictionary<int, List<QuadraticChromosome>> populationDictionary = new();
        algorithm.InitializePopulation();
        populationDictionary.Add(0, algorithm.Population);
        for (int i = 1; i < 11; i++)
        {
            algorithm.RunEvolution(1);

            // Algorithm keeps consistent population size
            populationDictionary.Add(i, algorithm.Population);
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
                var matchingChromosome = populationFromThisGen.FirstOrDefault(c => c.Equals(agent.Chromosome));
                if (matchingChromosome == null)
                {
                    Console.WriteLine($"Mismatch found for Chromosome: {agent.Chromosome}");
                    Console.WriteLine($"Agent Chromosome Fitness: {agent.Chromosome.Fitness}");
                    Console.WriteLine(
                        $"Population Chromosomes: {string.Join(", ", populationFromThisGen.Select(c => c.Fitness))}");
                }

                Assert.That(matchingChromosome, Is.Not.Null, $"Chromosome not found: {agent.Chromosome}");
            }
        }
    }

    [Test]
    public void GeneticAlgorithmLogger_CreateTsvSummariesHasCorrectChromosomes()
    {
        // Arrange  
        var parameters = new QuadraticGeneticAlgorithmParameters
        {
            PopulationSize = 4,
            MutationRate = 0.25,
            TournamentSize = 4,
            Generations = 100,
            OutputDirectory = OutputDirectory
        };

        var algorithm = new QuadraticRegressionGeneticAlgorithm(parameters);
        var logger = algorithm.Logger;

        List<GenerationLogDto> generations = new();
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

            Assert.That(averageChromosome, Is.EqualTo(generations[i].AverageChromosome));
            Assert.That(bestChromosome, Is.EqualTo(generations[i].BestChromosome));
        }

    }
}