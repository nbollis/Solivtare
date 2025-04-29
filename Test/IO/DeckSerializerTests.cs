using System.Text.Json;
using SolvitaireCore;
using SolvitaireIO;

namespace Test.Solitaire;

[TestFixture]
public class DeckSerializerVerboseTests
{
    [Test]
    public void StandardDeck_SerializeDeserialize_IsSame()
    {
        var deck = new StandardDeck();

        for (int i = 0; i < 10; i++)
        {
            deck.Shuffle();
            var json = DeckSerializer.SerializeDeck(deck);
            var deserialized = DeckSerializer.DeserializeStandardDeck(json);
            Assert.That(deck.Equals(deserialized));
        }
    }

    [Test]
    public void StandardDecks_SerializeDeserialize()
    {
        var deck = new StandardDeck();
        List<StandardDeck> decsk = new();
        for (int i = 0; i < 10; i++)
        {
            deck.Shuffle();
            decsk.Add(deck.Clone() as StandardDeck);
        }

        var file = Path.Combine(Path.GetTempPath(), "decks.json");
        File.WriteAllText(file, string.Empty);

        File.AppendAllText(file, DeckSerializer.SerializeStandardDecks(decsk));


        var json = File.ReadAllText(file);
        var decks = DeckSerializer.DeserializeStandardDecks(json);

        Assert.That(decks.Count, Is.EqualTo(decsk.Count));
        for (int i = 0; i < decsk.Count; i++)
        {
            Assert.That(decks[i].Equals(decsk[i]));
        }
    }

    [Test]
    public void SerializeDeck_ShouldIncludeSeedAndShuffles()
    {
        // Arrange
        var deck = new StandardDeck(42);

        // Act
        var json = DeckSerializer.SerializeDeck(deck);

        // Assert
        Assert.That(json, Does.Contain("\"Seed\": 42"));
        Assert.That(json, Does.Contain("\"Shuffles\": 0"));
    }

    [Test]
    public void SerializeDeck_ShouldReturnValidJson()
    {
        // Arrange
        var deck = new StandardDeck(42);
        deck.Shuffle();

        // Act
        var json = DeckSerializer.SerializeDeck(deck);

        // Assert
        Assert.That(() => JsonDocument.Parse(json), Throws.Nothing);
    }

    [Test]
    public void DeserializeStandardDeck_ShouldReturnEquivalentDeck()
    {
        // Arrange
        var originalDeck = new StandardDeck(42);
        originalDeck.Shuffle();
        var json = DeckSerializer.SerializeDeck(originalDeck);

        // Act
        var deserializedDeck = DeckSerializer.DeserializeStandardDeck(json);

        // Assert
        Assert.That(deserializedDeck.Seed, Is.EqualTo(originalDeck.Seed));
        Assert.That(deserializedDeck.Shuffles, Is.EqualTo(originalDeck.Shuffles));
        Assert.That(deserializedDeck.Cards, Is.EquivalentTo(originalDeck.Cards));
    }

    [Test]
    public void SerializeStandardDecks_ShouldReturnValidJson()
    {
        // Arrange
        var decks = new List<StandardDeck>
           {
               new StandardDeck(42),
               new StandardDeck(84)
           };

        // Act
        var json = DeckSerializer.SerializeStandardDecks(decks);

        // Assert
        Assert.That(() => JsonDocument.Parse(json), Throws.Nothing);
    }

    [Test]
    public void DeserializeStandardDecks_ShouldReturnEquivalentDecks()
    {
        // Arrange
        var originalDecks = new List<StandardDeck>
           {
               new StandardDeck(42),
               new StandardDeck(84)
           };
        var json = DeckSerializer.SerializeStandardDecks(originalDecks);

        // Act
        var deserializedDecks = DeckSerializer.DeserializeStandardDecks(json);

        // Assert
        Assert.That(deserializedDecks.Count, Is.EqualTo(originalDecks.Count));
        for (int i = 0; i < originalDecks.Count; i++)
        {
            Assert.That(deserializedDecks[i].Seed, Is.EqualTo(originalDecks[i].Seed));
            Assert.That(deserializedDecks[i].Shuffles, Is.EqualTo(originalDecks[i].Shuffles));
            Assert.That(deserializedDecks[i].Cards, Is.EquivalentTo(originalDecks[i].Cards));
        }
    }

    [Test]
    public void Serialize_CanReconstructWithoutCards()
    {
        int shuffleAmount = 20;
        var deck = new StandardDeck(2123);
        for (int i = 0; i < shuffleAmount; i++)
        {
            deck.Shuffle();
        }

        Assert.That(deck.Shuffles, Is.EqualTo(shuffleAmount));
        var json = DeckSerializer.SerializeDeck(deck);
        var deserializedDeck = DeckSerializer.DeserializeStandardDeck(json);
        Assert.That(deck.Seed, Is.EqualTo(deserializedDeck.Seed));
        Assert.That(deck.Shuffles, Is.EqualTo(deserializedDeck.Shuffles)); 
        Assert.That(deck.Cards.Count, Is.EqualTo(deserializedDeck.Cards.Count));
        for (int i = 0; i < deck.Cards.Count; i++)
        {
            Assert.That(deck.Cards[i].Equals(deserializedDeck.Cards[i]));
        }
        Assert.That(deck, Is.EqualTo(deserializedDeck.Cards));



        var reconstructed = new StandardDeck(2123);
        for (int i = 0; i < shuffleAmount; i++)
        {
            reconstructed.Shuffle();
        }

        Assert.That(deck.Seed, Is.EqualTo(reconstructed.Seed));
        Assert.That(deck.Shuffles, Is.EqualTo(reconstructed.Shuffles));
        Assert.That(deck.Cards.Count, Is.EqualTo(reconstructed.Cards.Count));
        for (int i = 0; i < deck.Cards.Count; i++)
        {
            Assert.That(deck.Cards[i].Equals(reconstructed.Cards[i]));
        }
        Assert.That(deck, Is.EqualTo(reconstructed.Cards));
    }
}

[TestFixture]
public class DeckSerializerMinimalTests
{
    [Test]
    public void StandardDeck_SerializeDeserialize_IsSame()
    {
        var deck = new StandardDeck();

        for (int i = 0; i < 10; i++)
        {
            deck.Shuffle();
            var json = DeckSerializer.SerializeMinimalDeck(deck);
            var deserialized = DeckSerializer.DeserializeMinimalDeck(json);
            Assert.That(deck.Equals(deserialized));
        }
    }

    [Test]
    public void StandardDecks_SerializeDeserialize()
    {
        var deck = new StandardDeck();
        List<StandardDeck> decsk = new();
        for (int i = 0; i < 10; i++)
        {
            deck.Shuffle();
            decsk.Add(deck.Clone() as StandardDeck);
        }

        var file = Path.Combine(Path.GetTempPath(), "decks.json");
        File.WriteAllText(file, string.Empty);

        File.AppendAllText(file, DeckSerializer.SerializeMinimalDecks(decsk));


        var json = File.ReadAllText(file);
        var decks = DeckSerializer.DeserializeMinimalDecks(json);

        Assert.That(decks.Count, Is.EqualTo(decsk.Count));
        for (int i = 0; i < decsk.Count; i++)
        {
            Assert.That(decks[i].Equals(decsk[i]));
        }
    }

    [Test]
    public void SerializeDeck_ShouldIncludeSeedAndShuffles()
    {
        // Arrange
        var deck = new StandardDeck(42);

        // Act
        var json = DeckSerializer.SerializeMinimalDeck(deck);

        // Assert
        Assert.That(json, Does.Contain("\"Seed\": 42"));
        Assert.That(json, Does.Contain("\"Shuffles\": 0"));
    }

    [Test]
    public void SerializeDeck_ShouldReturnValidJson()
    {
        // Arrange
        var deck = new StandardDeck(42);
        deck.Shuffle();

        // Act
        var json = DeckSerializer.SerializeMinimalDeck(deck);

        // Assert
        Assert.That(() => JsonDocument.Parse(json), Throws.Nothing);
    }

    [Test]
    public void DeserializeStandardDeck_ShouldReturnEquivalentDeck()
    {
        // Arrange
        var originalDeck = new StandardDeck(42);
        originalDeck.Shuffle();
        var json = DeckSerializer.SerializeMinimalDeck(originalDeck);

        // Act
        var deserializedDeck = DeckSerializer.DeserializeMinimalDeck(json);

        // Assert
        Assert.That(deserializedDeck.Seed, Is.EqualTo(originalDeck.Seed));
        Assert.That(deserializedDeck.Shuffles, Is.EqualTo(originalDeck.Shuffles));
        Assert.That(deserializedDeck.Cards, Is.EquivalentTo(originalDeck.Cards));
    }

    [Test]
    public void SerializeStandardDecks_ShouldReturnValidJson()
    {
        // Arrange
        var decks = new List<StandardDeck>
           {
               new StandardDeck(42),
               new StandardDeck(84)
           };

        // Act
        var json = DeckSerializer.SerializeMinimalDecks(decks);

        // Assert
        Assert.That(() => JsonDocument.Parse(json), Throws.Nothing);
    }

    [Test]
    public void DeserializeStandardDecks_ShouldReturnEquivalentDecks()
    {
        // Arrange
        var originalDecks = new List<StandardDeck>
           {
               new StandardDeck(42),
               new StandardDeck(84)
           };
        var json = DeckSerializer.SerializeMinimalDecks(originalDecks);

        // Act
        var deserializedDecks = DeckSerializer.DeserializeMinimalDecks(json);

        // Assert
        Assert.That(deserializedDecks.Count, Is.EqualTo(originalDecks.Count));
        for (int i = 0; i < originalDecks.Count; i++)
        {
            Assert.That(deserializedDecks[i].Seed, Is.EqualTo(originalDecks[i].Seed));
            Assert.That(deserializedDecks[i].Shuffles, Is.EqualTo(originalDecks[i].Shuffles));
            Assert.That(deserializedDecks[i].Cards, Is.EquivalentTo(originalDecks[i].Cards));
        }
    }

    [Test]
    public void Serialize_CanReconstructWithoutCards()
    {
        int shuffleAmount = 20;
        var deck = new StandardDeck(2123);
        for (int i = 0; i < shuffleAmount; i++)
        {
            deck.Shuffle();
        }

        Assert.That(deck.Shuffles, Is.EqualTo(shuffleAmount));
        var json = DeckSerializer.SerializeMinimalDeck(deck);
        var deserializedDeck = DeckSerializer.DeserializeMinimalDeck(json);
        Assert.That(deck.Seed, Is.EqualTo(deserializedDeck.Seed));
        Assert.That(deck.Shuffles, Is.EqualTo(deserializedDeck.Shuffles));
        Assert.That(deck.Cards.Count, Is.EqualTo(deserializedDeck.Cards.Count));
        for (int i = 0; i < deck.Cards.Count; i++)
        {
            Assert.That(deck.Cards[i].Equals(deserializedDeck.Cards[i]));
        }
        Assert.That(deck, Is.EqualTo(deserializedDeck.Cards));



        var reconstructed = new StandardDeck(2123);
        for (int i = 0; i < shuffleAmount; i++)
        {
            reconstructed.Shuffle();
        }

        Assert.That(deck.Seed, Is.EqualTo(reconstructed.Seed));
        Assert.That(deck.Shuffles, Is.EqualTo(reconstructed.Shuffles));
        Assert.That(deck.Cards.Count, Is.EqualTo(reconstructed.Cards.Count));
        for (int i = 0; i < deck.Cards.Count; i++)
        {
            Assert.That(deck.Cards[i].Equals(reconstructed.Cards[i]));
        }
        Assert.That(deck, Is.EqualTo(reconstructed.Cards));
    }
}