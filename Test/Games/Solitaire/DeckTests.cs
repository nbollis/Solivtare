using System.Text.Json;
using SolvitaireCore;
using SolvitaireIO;

namespace Test.Games.Solitaire;

[TestFixture]
public class DeckTests
{
    [Test]
    public void DrawCard_EmptyDeck_ShouldThrowInvalidOperationException()
    {
        // Arrange  
        var deck = new TestDeck();

        // Act & Assert  
        Assert.That(() => deck.DrawCard(), Throws.InvalidOperationException);
    }

    [Test]
    public void DrawCard_ValidDeck_ShouldReturnCardAndRemoveItFromDeck()
    {
        // Arrange  
        var deck = new TestDeck();
        var card = new TestCard(Suit.Hearts, Rank.Ace);

        deck.Cards.Add(card);

        // Act  
        var drawnCard = deck.DrawCard();

        // Assert  
        Assert.That(drawnCard, Is.EqualTo(card));
        Assert.That(deck.Cards, Does.Not.Contain(card));
    }

    [Test]
    public void Shuffle_ShouldRandomizeCardOrder()
    {
        // Arrange  
        var deck = new TestDeck();
        var cards = new List<TestCard>
       {
           new TestCard(Suit.Hearts, Rank.Ace),
           new TestCard(Suit.Spades, Rank.Three),
           new TestCard(Suit.Spades, Rank.Two),
           new TestCard(Suit.Spades, Rank.Five),
           new TestCard(Suit.Spades, Rank.Seven),
           new TestCard(Suit.Spades, Rank.King),
           new TestCard(Suit.Diamonds, Rank.Queen)
       };
        deck.Cards.AddRange(cards);

        // Act  
        deck.Shuffle();

        // Assert  
        Assert.That(deck.Cards, Is.Not.EqualTo(cards));
        Assert.That(deck.Cards, Is.EquivalentTo(cards));
    }

    [Test]
    public void Deck_Clone_IsSame()
    {
        var deck = new StandardDeck();
        deck.Shuffle();

        var clone = deck.Clone() as StandardDeck;

        Assert.That(clone, Is.Not.Null);
        Assert.That(clone.Cards.Count, Is.EqualTo(deck.Cards.Count));

        for (int i = 0; i < deck.Cards.Count; i++)
        {
            Assert.That(deck[i], Is.EqualTo(clone[i]));
        }
    }
}