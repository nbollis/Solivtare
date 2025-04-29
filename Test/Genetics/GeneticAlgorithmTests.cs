using NUnit.Framework;
using SolvitaireGenetics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Test;

namespace Tests.Genetics;

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
}
