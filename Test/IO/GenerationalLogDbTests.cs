using SolvitaireIO.Database;
using SolvitaireIO.Database.Models;
using SolvitaireIO.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.IO;

[TestFixture]
public class GenerationalLogDbTests
{
    [Test]
    public async Task Test_AddGenerationLogAsync_WithFullDummyData()
    {
        // Arrange  
        var context = SolvitaireDbContext.CreateInMemoryDbContext();

        // Apply the schema to the in-memory database  
        await context.Database.EnsureCreatedAsync();

        var repository = new GenerationLogRepository(context);

        var log = new GenerationLog
        {
            Generation = 1,
            GenerationFinishedTime = DateTime.UtcNow,
            BestFitness = 0.95,
            AverageFitness = 0.85,
            StdFitness = 0.1,
            AveragePairwiseDiversity = 0.5,
            VarianceFromAverageChromosome = 0.02,
            SpeciesCount = 3,
            IntraSpeciesDiversity = 0.4,
            InterSpeciesDiversity = 0.6,
            BestChromosomeJson = "{\"genes\": [1, 2, 3]}",
            AverageChromosmeJson = "{\"genes\": [2, 3, 4]}",
            StdChromosmeJson = "{\"genes\": [0.5, 0.6, 0.7]}"
        };

        // Act  
        await repository.AddGenerationLogAsync(log);
        var logs = await repository.GetAllGenerationLogsAsync();

        // Assert  
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0].BestFitness, Is.EqualTo(0.95));
        Assert.That(logs[0].AverageFitness, Is.EqualTo(0.85));
        Assert.That(logs[0].StdFitness, Is.EqualTo(0.1));
        Assert.That(logs[0].AveragePairwiseDiversity, Is.EqualTo(0.5));
        Assert.That(logs[0].VarianceFromAverageChromosome, Is.EqualTo(0.02));
        Assert.That(logs[0].SpeciesCount, Is.EqualTo(3));
        Assert.That(logs[0].IntraSpeciesDiversity, Is.EqualTo(0.4));
        Assert.That(logs[0].InterSpeciesDiversity, Is.EqualTo(0.6));
        Assert.That(logs[0].BestChromosomeJson, Is.EqualTo("{\"genes\": [1, 2, 3]}"));
        Assert.That(logs[0].AverageChromosmeJson, Is.EqualTo("{\"genes\": [2, 3, 4]}"));
        Assert.That(logs[0].StdChromosmeJson, Is.EqualTo("{\"genes\": [0.5, 0.6, 0.7]}"));
    }
}

