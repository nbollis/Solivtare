using System.Text.Json;
using SolvitaireCore;
using SolvitaireIO;

namespace Test.IO;

[TestFixture]
public class SimpleDeckFileTests
{
    private const string TestFilePath = "test_decks.json";
    private SimpleDeckFile _deckFile;

    [SetUp]
    public void SetUp()
    {
        // Ensure the test file is clean before each test  
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }

        _deckFile = new SimpleDeckFile(TestFilePath);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the test file after each test  
        if (File.Exists(TestFilePath))
        {
            File.Delete(TestFilePath);
        }
    }

    [Test]
    public void Constructor_FileDoesNotExist_ShouldCreateFile()
    {
        // Arrange  
        var filePath = "new_test_decks.json";

        // Act  
        var deckFile = new SimpleDeckFile(filePath);

        // Assert  
        Assert.That(File.Exists(filePath), Is.True);

        // Cleanup  
        File.Delete(filePath);
    }

    [Test]
    public void ReadAllDecks_EmptyFile_ShouldReturnEmptyList()
    {
        // Act  
        var decks = _deckFile.ReadAllDecks();

        // Assert  
        Assert.That(decks, Is.Empty);
    }

    [Test]
    public void AddDeck_ShouldAddDeckToFile()
    {
        // Arrange  
        var deck = new StandardDeck();

        // Act  
        _deckFile.AddDeck(deck);
        var decks = _deckFile.ReadAllDecks();

        // Assert  
        Assert.That(decks.Count, Is.EqualTo(1));
        Assert.That(decks[0].Seed, Is.EqualTo(deck.Seed));
    }

    [Test]
    public void AddDecks_ShouldAddMultipleDecksToFile()
    {
        // Arrange  
        var deck1 = new StandardDeck(1);
        var deck2 = new StandardDeck(2);

        // Act  
        _deckFile.AddDecks(new[] { deck1, deck2 });
        var decks = _deckFile.ReadAllDecks();

        // Assert  
        Assert.That(decks.Count, Is.EqualTo(2));
        Assert.That(decks[0].Seed, Is.EqualTo(deck1.Seed));
        Assert.That(decks[1].Seed, Is.EqualTo(deck2.Seed));
    }

    [Test]
    public void AddDecks_ExistingDecks_ShouldAppendToExistingDecks()
    {
        // Arrange  
        var initialDeck = new StandardDeck(1);
        _deckFile.AddDeck(initialDeck);

        var newDecks = new[] { new StandardDeck(2), new StandardDeck(3) };

        // Act  
        _deckFile.AddDecks(newDecks);
        var decks = _deckFile.ReadAllDecks();

        // Assert  
        Assert.That(decks.Count, Is.EqualTo(3));
        Assert.That(decks[0].Seed, Is.EqualTo(initialDeck.Seed));
        Assert.That(decks[1].Seed, Is.EqualTo(newDecks[0].Seed));
        Assert.That(decks[2].Seed, Is.EqualTo(newDecks[1].Seed));
    }
}
