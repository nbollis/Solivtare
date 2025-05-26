using SolvitaireCore;

namespace Test.Games.Solitaire;

[TestFixture]
public class SuitExtensionsTests
{
    [Test]
    public void ToSuitColor_Hearts_ShouldReturnRed()
    {
        // Arrange  
        var suit = Suit.Hearts;

        // Act  
        var result = suit.ToSuitColor();

        // Assert  
        Assert.That(result, Is.EqualTo(Color.Red));
    }

    [Test]
    public void ToSuitColor_Diamonds_ShouldReturnRed()
    {
        // Arrange  
        var suit = Suit.Diamonds;

        // Act  
        var result = suit.ToSuitColor();

        // Assert  
        Assert.That(result, Is.EqualTo(Color.Red));
    }

    [Test]
    public void ToSuitColor_Clubs_ShouldReturnBlack()
    {
        // Arrange  
        var suit = Suit.Clubs;

        // Act  
        var result = suit.ToSuitColor();

        // Assert  
        Assert.That(result, Is.EqualTo(Color.Black));
    }

    [Test]
    public void ToSuitColor_Spades_ShouldReturnBlack()
    {
        // Arrange  
        var suit = Suit.Spades;

        // Act  
        var result = suit.ToSuitColor();

        // Assert  
        Assert.That(result, Is.EqualTo(Color.Black));
    }

    [Test]
    public void ToSuitColor_InvalidSuit_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange  
        var invalidSuit = (Suit)999;

        // Act & Assert  
        Assert.That(() => invalidSuit.ToSuitColor(), Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
