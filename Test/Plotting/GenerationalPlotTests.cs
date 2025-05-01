using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolvitaireIO;

namespace Test.Plotting;


[TestFixture]
public class GenerationalPlotTests
{
    public string GenerationalLogPath;
    public string AgentLogPath;

    [OneTimeSetUp]
    public void Setup()
    {
        GenerationalLogPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "GenerationalLog.json");
        AgentLogPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "AgentLog.json");

        // Ensure the files exist
        if (!File.Exists(GenerationalLogPath))
        {
            File.WriteAllText(GenerationalLogPath, "[]"); // Initialize with an empty JSON array
        }
        if (!File.Exists(AgentLogPath))
        {
            File.WriteAllText(AgentLogPath, "[]"); // Initialize with an empty JSON array
        }
    }

    [Test]
    public void FitnessByGeneration()
    {
        string path = @"A:\Projects and Original Works\Solvitaire\WonDeckStats.json";
        var deckStats = File.ReadAllText(path);
        var temp = DeckSerializer.DeserializeDeckStatisticsList(deckStats);
        var temp2 = new DeckStatisticsFile(path);
        var temp3 = temp2.ReadAllDecks();
    }
}

