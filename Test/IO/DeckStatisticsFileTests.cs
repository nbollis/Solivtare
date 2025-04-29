using SolvitaireCore;
using SolvitaireIO;

namespace Test.IO;

[TestFixture]
public class DeckStatisticsFileTests
{
    private const string TestFilePath = "testDeckStatistics.json";
    private DeckStatisticsFile _deckStatisticsFile;

    [SetUp]
    public void SetUp()
    {
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
        _deckStatisticsFile = new DeckStatisticsFile(TestFilePath);
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
    public void Constructor_FileDoesNotExist_CreatesEmptyFile()
    {
        // Act  
        var fileExists = File.Exists(TestFilePath);
        var fileContent = File.ReadAllText(TestFilePath);

        // Assert  
        Assert.That(fileExists, Is.True);
        Assert.That(fileContent, Is.EqualTo("[]"));
    }

    [Test]
    public void AddDeck_NewDeck_DeckIsAdded()
    {
        // Arrange  
        var deck = new StandardDeck();

        // Act  
        _deckStatisticsFile.AddDeck(deck);
        var allDecks = _deckStatisticsFile.ReadAllDecks();

        // Assert  
        Assert.That(allDecks.Count, Is.EqualTo(1));
        Assert.That(allDecks[0], Is.EqualTo(deck));
    }

    [Test]
    public void AddOrUpdateWinnableDeck_NewDeck_DeckStatisticsAreAdded()
    {
        // Arrange  
        var deck = new StandardDeck();

        // Act  
        _deckStatisticsFile.AddOrUpdateWinnableDeck(deck, 10, true);
        var allDeckStatistics = _deckStatisticsFile.ReadAllDeckStatistics();

        // Assert  
        Assert.That(allDeckStatistics.Count, Is.EqualTo(1));
        Assert.That(allDeckStatistics[0].Deck, Is.EqualTo(deck));
        Assert.That(allDeckStatistics[0].TimesWon, Is.EqualTo(1));
        Assert.That(allDeckStatistics[0].TimesPlayed, Is.EqualTo(1));
        Assert.That(allDeckStatistics[0].FewestMovesToWin, Is.EqualTo(10));
        Assert.That(allDeckStatistics[0].MovesPerAttempt, Is.EquivalentTo(new List<int> { 10 }));
        Assert.That(allDeckStatistics[0].MovesPerWin, Is.EquivalentTo(new List<int> { 10 }));
    }

    [Test]
    public void Flush_UnsavedChanges_FileIsUpdated()
    {
        // Arrange  
        var deck = new StandardDeck();
        _deckStatisticsFile.AddOrUpdateWinnableDeck(deck, 10, true);

        // Act  
        _deckStatisticsFile.Flush();
        var fileContent = File.ReadAllText(TestFilePath);
        var deserializedData = DeckSerializer.DeserializeDeckStatisticsList(fileContent);

        // Assert  
        Assert.That(deserializedData.Count, Is.EqualTo(1));
        Assert.That(deserializedData[0].Deck, Is.EqualTo(deck));
        Assert.That(deserializedData[0].TimesWon, Is.EqualTo(1));
        Assert.That(deserializedData[0].TimesPlayed, Is.EqualTo(1));
        Assert.That(deserializedData[0].FewestMovesToWin, Is.EqualTo(10));
    }

    [Test]
    public void AddDecks_MultipleDecks_AllDecksAreAdded()
    {
        // Arrange  
        var deck1 = new StandardDeck(1);
        var deck2 = new StandardDeck(2);
        var decks = new List<StandardDeck> { deck1, deck2 };

        // Act  
        _deckStatisticsFile.AddDecks(decks);
        var allDecks = _deckStatisticsFile.ReadAllDecks();

        // Assert  
        Assert.That(allDecks.Count, Is.EqualTo(2));
        Assert.That(allDecks, Does.Contain(deck1));
        Assert.That(allDecks, Does.Contain(deck2));
    }

    [Test]
    public void AddOrUpdateWinnableDeck_MultipleAttempts_StatisticsAreAccurate()
    {
        // Arrange  
        var deck = new StandardDeck();
        _deckStatisticsFile.AddOrUpdateWinnableDeck(deck, 15, true);
        _deckStatisticsFile.AddOrUpdateWinnableDeck(deck, 20, false);
        _deckStatisticsFile.AddOrUpdateWinnableDeck(deck, 10, true);

        // Act  
        var allDeckStatistics = _deckStatisticsFile.ReadAllDeckStatistics();

        // Assert  
        Assert.That(allDeckStatistics.Count, Is.EqualTo(1));
        Assert.That(allDeckStatistics[0].Deck, Is.EqualTo(deck));
        Assert.That(allDeckStatistics[0].TimesWon, Is.EqualTo(2));
        Assert.That(allDeckStatistics[0].TimesPlayed, Is.EqualTo(3));
        Assert.That(allDeckStatistics[0].FewestMovesToWin, Is.EqualTo(10));
        Assert.That(allDeckStatistics[0].MovesPerAttempt, Is.EquivalentTo(new List<int> { 15, 20, 10 }));
        Assert.That(allDeckStatistics[0].MovesPerWin, Is.EquivalentTo(new List<int> { 15, 10 }));
    }

    [Test]
    public void Constructor_FileExists_LoadsDataIntoCache()
    {
        // Arrange  
        var deck = new StandardDeck();
        _deckStatisticsFile.AddOrUpdateWinnableDeck(deck, 10, true);
        _deckStatisticsFile.Flush();

        // Act  
        var newDeckStatisticsFile = new DeckStatisticsFile(TestFilePath);
        var allDeckStatistics = newDeckStatisticsFile.ReadAllDeckStatistics();

        // Assert  
        Assert.That(allDeckStatistics.Count, Is.EqualTo(1));
        Assert.That(allDeckStatistics[0].Deck, Is.EqualTo(deck));
        Assert.That(allDeckStatistics[0].TimesWon, Is.EqualTo(1));
        Assert.That(allDeckStatistics[0].TimesPlayed, Is.EqualTo(1));
        Assert.That(allDeckStatistics[0].FewestMovesToWin, Is.EqualTo(10));
    }

    [Test]
    public void ReadAllDecks_NoDecks_ReturnsEmptyList()
    {
        // Act  
        var allDecks = _deckStatisticsFile.ReadAllDecks();

        // Assert  
        Assert.That(allDecks, Is.Empty);
    }

    [Test]
    public void ReadAllDeckStatistics_NoDecks_ReturnsEmptyList()
    {
        // Act  
        var allDeckStatistics = _deckStatisticsFile.ReadAllDeckStatistics();

        // Assert  
        Assert.That(allDeckStatistics, Is.Empty);
    }

    [Test]
    [TestCase(23, 4)]
    public void ReadAllDecks_RestoresSeedAndShuffleInformation(int seed, int shuffles)
    {
        var deck = new StandardDeck(seed);
        int movesMade = seed * shuffles;
        for (int i = 0; i < shuffles; i++)
        {
            deck.Shuffle();
        }
        Assert.That(deck.Shuffles, Is.EqualTo(shuffles));

        _deckStatisticsFile.AddOrUpdateWinnableDeck(deck, movesMade, true);
        _deckStatisticsFile.Flush();

        var newDeckStatisticsFile = new DeckStatisticsFile(TestFilePath);
        var allDeckStatistics = newDeckStatisticsFile.ReadAllDeckStatistics();
        Assert.That(allDeckStatistics.Count, Is.EqualTo(1));
        Assert.That(allDeckStatistics[0].Deck, Is.EqualTo(deck));
    }
}
