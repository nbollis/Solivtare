﻿using Microsoft.EntityFrameworkCore;
using SolvitaireIO.Database;
using SolvitaireIO.Database.Models;
using SolvitaireIO.Database.Repositories;

namespace Test.IO;

[TestFixture]
public class GenerationalLogDbTests
{

    private SolvitaireDbContext _context = null!;
    private GenerationLogRepository _generationRepository = null!;
    private AgentLogRepository _agentRepository = null!;
    private ChromosomeRepository _chromosomeRepository = null!;

    [SetUp]
    public async Task SetUp()
    {
        // Create a unique in-memory database for each test
        _context = SolvitaireDbContext.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        await _context.Database.EnsureCreatedAsync();

        _generationRepository = new GenerationLogRepository(_context);
        _agentRepository = new AgentLogRepository(_context);
        _chromosomeRepository = new ChromosomeRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    [Test]
    public async Task Test_AddGenerationLogAsync_WithFullDummyData()
    {
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
            BestChromosomeId = "",
            AverageChromosomeId = "",
            StdChromosomeId = "",
        };

        // Act  
        await _generationRepository.AddGenerationAsync(log);
        var logs = await _generationRepository.GetAllGenerationLogsAsync();

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
  
    }

    [Test]
    public void Test_AddGenerationLogAsync_WithAgents()
    {
        var generationLog = new GenerationLog
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
            BestChromosomeId = "",
            AverageChromosomeId = "",
            StdChromosomeId = "",
        };

        var agentLog = new AgentLog
        {
            Generation = 1,
            Fitness = 0.9,
            GamesPlayed = 10,
            GamesWon = 5,
            MovesMade = 50,
            Chromosome = ChromosomeLog.FromChromosome(new TestChromosome() { MutableStatsByName = new() { { "a", 1 } } }),
        };

        // Act
         _generationRepository.AddGenerationAsync(generationLog).Wait();
         _agentRepository.AddAgentAsync(agentLog).Wait();

        var retrievedGenerationLog =  _generationRepository.GetGenerationLogWithAgentsAsync(1).Result;

        // Assert
        Assert.That(retrievedGenerationLog, Is.Not.Null);
        Assert.That(retrievedGenerationLog!.AgentLogs.Count, Is.EqualTo(1));
        Assert.That(retrievedGenerationLog.AgentLogs.First().Fitness, Is.EqualTo(0.9));
    }

    [Test]
    public async Task Test_GenerationLog_WithChromosomes()
    {
        // Arrange
        var generationLog = new GenerationLog
        {
            Generation = 1,
            GenerationFinishedTime = DateTime.UtcNow,
            BestFitness = 0.95,
            AverageFitness = 0.85,
            StdFitness = 0.1,
            BestChromosome = ChromosomeLog.FromChromosome(new TestChromosome(){ MutableStatsByName = new (){{"a", 1}}}),
            AverageChromosome = ChromosomeLog.FromChromosome(new TestChromosome() { MutableStatsByName = new() { { "a", 2 } } }),
            StdChromosome = ChromosomeLog.FromChromosome(new TestChromosome() { MutableStatsByName = new() { { "a", 3 } } }),
        };

        // Act
        _generationRepository.AddGenerationAsync(generationLog).Wait();

        var retrievedGenerationLog = _generationRepository.GetLastGenerationAsync().Result;

        // Assert
        Assert.That(retrievedGenerationLog, Is.Not.Null);
        Assert.That(retrievedGenerationLog!.BestChromosome, Is.Not.Null);
        Assert.That(retrievedGenerationLog.BestChromosome!.Chromosome.GetWeight("a"), Is.EqualTo(1));
        Assert.That(retrievedGenerationLog.AverageChromosome, Is.Not.Null);
        Assert.That(retrievedGenerationLog.AverageChromosome!.Chromosome.GetWeight("a"), Is.EqualTo(2));
        Assert.That(retrievedGenerationLog.StdChromosome, Is.Not.Null);
        Assert.That(retrievedGenerationLog.StdChromosome!.Chromosome.GetWeight("a"), Is.EqualTo(3));
    }
}

