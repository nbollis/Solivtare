using SolvitaireCore;

namespace Test.Solitaire;

[TestFixture]
public class StandardDeckTests
{
    [Test]
    public void StandardDeck_Constructor_ShouldCreate52UniqueCards()
    {
        // Arrange & Act  
        var deck = new StandardDeck();

        // Assert  
        Assert.That(deck.Cards.Count, Is.EqualTo(52));
        Assert.That(deck.Cards.Distinct().Count(), Is.EqualTo(52));
    }

    [Test]
    public void StandardDeck_Constructor_ShouldContainAllSuitsAndRanks()
    {
        // Arrange & Act  
        var deck = new StandardDeck();

        // Assert  
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                Assert.That(deck.Cards, Does.Contain(new Card(suit, rank)));
            }
        }
    }
}