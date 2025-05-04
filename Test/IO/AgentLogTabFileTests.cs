using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SolvitaireGenetics;

namespace Test.IO;

[TestFixture]
public class AgentLogTabFileTests
{
    private const string TestFilePath = "test_agent_logs.tsv";

    [SetUp]
    public void SetUp()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }

    [Test]
    public void WriteToFile_ShouldWriteCorrectHeadersAndData()
    {
        // Arrange  
        var chromosome = new QuadraticChromosome()
        {
            MutableStatsByName = new Dictionary<string, double>
           {
               { "Speed", 1.5 },
               { "Strength", 3.0 }
           }
        };

        var agentLogs = new List<AgentLog>
       {
           new AgentLog
           {
               Generation = 1,
               Count = 10,
               Fitness = 95.5,
               GamesWon = 5,
               MovesMade = 50,
               GamesPlayed = 20,
               Chromosome = chromosome
           }
       };

        // Act  
        AgentLogTabFile.WriteToFile(TestFilePath, agentLogs);

        // Assert  
        Assert.That(File.Exists(TestFilePath), Is.True);

        var lines = File.ReadAllLines(TestFilePath);
        Assert.That(lines.Length, Is.EqualTo(2));

        var expectedHeader = "Generation\tCount\tFitness\tGamesWon\tMovesMade\tGamesPlayed\tChromosomeType\tSpeed\tStrength";
        Assert.That(lines[0], Is.EqualTo(expectedHeader));

        var expectedData = "1\t10\t95.5\t5\t50\t20\tSolvitaireGenetics.QuadraticChromosome\t1.5\t3";
        Assert.That(lines[1], Is.EqualTo(expectedData));
    }

    [Test]
    public void ReadFromFile_ShouldReadCorrectHeadersAndData()
    {
        // Arrange  
        var content =
            "Generation\tCount\tFitness\tGamesWon\tMovesMade\tGamesPlayed\tChromosomeType\tSpeed\tStrength\n" +
            "1\t10\t95.5\t5\t50\t20\tSolvitaireGenetics.QuadraticChromosome\t1.5\t3";
        File.WriteAllText(TestFilePath, content);

        // Act  
        var agentLogs = AgentLogTabFile.ReadFromFile(TestFilePath);

        // Assert  
        Assert.That(agentLogs.Count, Is.EqualTo(1));

        var log = agentLogs[0];
        Assert.That(log.Generation, Is.EqualTo(1));
        Assert.That(log.Count, Is.EqualTo(10));
        Assert.That(log.Fitness, Is.EqualTo(95.5));
        Assert.That(log.GamesWon, Is.EqualTo(5));
        Assert.That(log.MovesMade, Is.EqualTo(50));
        Assert.That(log.GamesPlayed, Is.EqualTo(20));
        Assert.That(log.Chromosome, Is.TypeOf<QuadraticChromosome>());
        Assert.That(log.Chromosome.MutableStatsByName["Speed"], Is.EqualTo(1.5));
        Assert.That(log.Chromosome.MutableStatsByName["Strength"], Is.EqualTo(3.0));
    }

    [Test]
    public void ReadFromFile_InvalidHeader_ShouldThrowException()
    {
        // Arrange  
        var content = "InvalidHeader\n1\t10\t95.5\t5\t50\t20\tSolvitaireGenetics.QuadraticChromosome\t1.5\t3";
        File.WriteAllText(TestFilePath, content);

        // Act & Assert  
        var ex = Assert.Throws<InvalidDataException>(() => AgentLogTabFile.ReadFromFile(TestFilePath));
        Assert.That(ex.Message, Is.EqualTo("The file does not have a valid header."));
    }

    [Test]
    public void ReadFromFile_InvalidRow_ShouldThrowException()
    {
        // Arrange  
        var content =
            "Generation\tCount\tFitness\tGamesWon\tMovesMade\tGamesPlayed\tChromosomeType\tSpeed\tStrength\n" +
            "1\t10\t95.5\t5\t50\t20";
        File.WriteAllText(TestFilePath, content);

        // Act & Assert  
        var ex = Assert.Throws<InvalidDataException>(() => AgentLogTabFile.ReadFromFile(TestFilePath));
        Assert.That(ex.Message, Is.EqualTo("The file contains an invalid row."));
    }
}
