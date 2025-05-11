using NUnit.Framework;
using SolvitaireGenetics;
using SolvitaireIO.Database.Models;

namespace Test.IO;

[TestFixture]
public class AgentLogTests
{
    [Test]
    public void AgentLog_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange  
        var agentLog = new AgentLog();

        // Act & Assert  
        Assert.That(agentLog.Generation, Is.EqualTo(0));
        Assert.That(agentLog.Fitness, Is.EqualTo(0.0));
        Assert.That(agentLog.GamesWon, Is.EqualTo(0));
        Assert.That(agentLog.MovesMade, Is.EqualTo(0));
        Assert.That(agentLog.GamesPlayed, Is.EqualTo(0));
        Assert.That(agentLog.Chromosome, Is.Null);
    }

    [Test]
    public void AgentLog_SetProperties_ShouldReflectCorrectValues()
    {
        // Arrange  
        var chromosome = new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Stat1", 1.0 }, { "Stat2", 2.0 } } };
        var agentLog = new AgentLog
        {
            Generation = 1,
            Fitness = 95.5,
            GamesWon = 10,
            MovesMade = 50,
            GamesPlayed = 20,
            Chromosome = ChromosomeLog.FromChromosome(chromosome)
        };

        // Act & Assert  
        Assert.That(agentLog.Generation, Is.EqualTo(1));
        Assert.That(agentLog.Fitness, Is.EqualTo(95.5));
        Assert.That(agentLog.GamesWon, Is.EqualTo(10));
        Assert.That(agentLog.MovesMade, Is.EqualTo(50));
        Assert.That(agentLog.GamesPlayed, Is.EqualTo(20));
        Assert.That(agentLog.Chromosome.Chromosome, Is.EqualTo(chromosome));
    }

    [Test]
    public void AgentLog_ChromosomeWeights_ShouldBeSetCorrectly()
    {
        // Arrange  
        var chromosome = new TestChromosome { MutableStatsByName = new Dictionary<string, double> { { "Speed", 1.5 }, { "Strength", 3.0 } } };
        var agentLog = new AgentLog { Chromosome = ChromosomeLog.FromChromosome(chromosome) };

        // Act & Assert  
        Assert.That(agentLog.Chromosome.Chromosome.MutableStatsByName["Speed"], Is.EqualTo(1.5));
        Assert.That(agentLog.Chromosome.Chromosome.MutableStatsByName["Strength"], Is.EqualTo(3.0));
    }
}
