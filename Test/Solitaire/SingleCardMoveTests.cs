using SolvitaireCore;

namespace Test.Solitaire;

[TestFixture]
public class SingleCardMoveTests
{
    [Test]
    public void SingleCardMove_IsValid_ValidMoveToFoundation_ShouldReturnTrue()
    {
        // Arrange
        var foundationPile = new FoundationPile(Suit.Hearts);
        var tableauPile = new TableauPile(0, new List<Card> { new Card(Suit.Hearts, Rank.Ace) });
        var card = tableauPile.TopCard;
        var move = new SingleCardMove(tableauPile, foundationPile, card);

        // Act
        var result = move.IsValid(new GameState());

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SingleCardMove_IsValid_InvalidMoveToFoundation_ShouldReturnFalse()
    {
        // Arrange
        var foundationPile = new FoundationPile(Suit.Hearts);
        var tableauPile = new TableauPile(0, new List<Card> { new Card(Suit.Spades, Rank.Ace) });
        var card = tableauPile.TopCard;
        var move = new SingleCardMove(tableauPile, foundationPile, card);

        // Act
        var result = move.IsValid(new GameState());

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SingleCardMove_IsValid_ValidMoveToTableau_ShouldReturnTrue()
    {
        // Arrange
        var from = new TableauPile(0, new List<Card> { new Card(Suit.Hearts, Rank.Queen) });
        var to = new TableauPile(1, new List<Card> { new Card(Suit.Spades, Rank.King) });
        var card = from.TopCard;
        var move = new SingleCardMove(from, to, card);

        // Act
        var result = move.IsValid(new GameState());

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SingleCardMove_IsValid_InvalidMoveToTableau_ShouldReturnFalse()
    {
        // Arrange
        var to = new TableauPile(0, new List<Card> { new Card(Suit.Hearts, Rank.Queen) });
        var from = new TableauPile(1, new List<Card> { new Card(Suit.Hearts, Rank.King) });
        var card = from.TopCard;
        var move = new SingleCardMove(from, to, card);

        // Act
        var result = move.IsValid(new GameState());

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SingleCardMove_IsValid_MoveToWaste_ShouldReturnTrue()
    {
        // Arrange
        var wastePile = new WastePile();
        var stockPile = new StockPile(new List<Card> { new Card(Suit.Clubs, Rank.Ten) });
        var card = stockPile.TopCard;
        var move = new SingleCardMove(stockPile, wastePile, card);

        // Act
        var result = move.IsValid(new GameState());

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SingleCardMove_IsValid_MoveToStock_ShouldReturnFalse()
    {
        // Arrange
        var stockPile = new StockPile();
        var tableauPile = new TableauPile(0, new List<Card> { new Card(Suit.Spades, Rank.Ace) });
        var card = tableauPile.TopCard;
        var move = new SingleCardMove(tableauPile, stockPile, card);

        // Act
        var result = move.IsValid(new GameState());

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SingleCardMove_Execute_ValidMove_ShouldMoveCard()
    {
        // Arrange
        var foundationPile = new FoundationPile(Suit.Hearts);
        var tableauPile = new TableauPile(0, new List<Card> { new Card(Suit.Hearts, Rank.Ace) });
        var card = tableauPile.TopCard;
        var move = new SingleCardMove(tableauPile, foundationPile, card);

        // Act
        move.Execute(new GameState());

        // Assert
        Assert.That(tableauPile.Cards.Count, Is.EqualTo(0));
        Assert.That(foundationPile.TopCard, Is.EqualTo(card));
    }

    [Test]
    public void SingleCardMove_Execute_InvalidMove_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var foundationPile = new FoundationPile(Suit.Hearts);
        var tableauPile = new TableauPile(0, new List<Card> { new Card(Suit.Spades, Rank.Ace) });
        var card = tableauPile.TopCard;
        var move = new SingleCardMove(tableauPile, foundationPile, card);

        // Act & Assert
        Assert.That(() => move.Execute(new GameState()), Throws.InvalidOperationException);
    }

    [Test]
    public void SingleCardMove_ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var card = new Card(Suit.Hearts, Rank.Ace);
        var fromPile = new TableauPile();
        var toPile = new FoundationPile(Suit.Hearts, [card]);
        var move = new SingleCardMove(fromPile, toPile, card);

        // Act
        var result = move.ToString();

        // Assert
        Assert.That(result, Is.EqualTo($"Move {card} from {fromPile} to {toPile}"));
    }
}
