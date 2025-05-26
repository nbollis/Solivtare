using SolvitaireCore;

namespace Test.Games.Solitaire;

[TestFixture]
public class SingleCardMoveTests
{
    [Test]
    public void SingleCardMove_IsValid_ValidMoveToFoundation_ShouldReturnTrue()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var foundationPile = gameState.FoundationPiles.First(p => p.Suit == Suit.Hearts);
        var tableauPile = gameState.TableauPiles[0];
        tableauPile.Cards.Add(new Card(Suit.Hearts, Rank.Ace, true));
        var card = tableauPile.TopCard!;
        var move = new SingleCardMove(tableauPile.Index, foundationPile.Index, card);

        // Act
        var result = move.IsValid(gameState);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SingleCardMove_IsValid_InvalidMoveToFoundation_ShouldReturnFalse()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var foundationPile = gameState.FoundationPiles.First(p => p.Suit == Suit.Hearts);
        var tableauPile = gameState.TableauPiles[0];
        tableauPile.Cards.Add(new Card(Suit.Spades, Rank.Ace, true));
        var card = tableauPile.TopCard!;
        var move = new SingleCardMove(tableauPile.Index, foundationPile.Index, card);

        // Act
        var result = move.IsValid(gameState);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SingleCardMove_IsValid_ValidMoveToTableau_ShouldReturnTrue()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var from = gameState.TableauPiles[0];
        var to = gameState.TableauPiles[1];
        from.Cards.Add(new Card(Suit.Hearts, Rank.Three, false));
        from.Cards.Add(new Card(Suit.Hearts, Rank.Queen, true));
        to.TryAddCard(new Card(Suit.Spades, Rank.King, true));
        var card = from.TopCard!;
        var move = new SingleCardMove(from.Index, to.Index, card);

        // Act
        Assert.That(from.BottomCard.IsFaceUp, Is.False);
        var result = move.IsValid(gameState);
        gameState.ExecuteMove(move);

        Assert.That(from.BottomCard.IsFaceUp, Is.True);
        gameState.UndoMove(move);
        Assert.That(from.BottomCard.IsFaceUp, Is.False);



        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SingleCardMove_IsValid_InvalidMoveToTableau_ShouldReturnFalse()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var from = gameState.TableauPiles[0];
        var to = gameState.TableauPiles[1];
        from.Cards.Add(new Card(Suit.Hearts, Rank.King, true));
        to.Cards.Add(new Card(Suit.Hearts, Rank.Queen, true));
        var card = from.TopCard!;
        var move = new SingleCardMove(from.Index, to.Index, card);

        // Act
        var result = move.IsValid(gameState);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SingleCardMove_IsValid_MoveToWaste_ShouldReturnTrue()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var stockPile = gameState.StockPile;
        var wastePile = gameState.WastePile;
        stockPile.Cards.Add(new Card(Suit.Clubs, Rank.Ten, true));
        var card = stockPile.TopCard!;
        var move = new SingleCardMove(stockPile.Index, wastePile.Index, card);

        // Act
        var result = move.IsValid(gameState);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SingleCardMove_IsValid_MoveToStock_ShouldReturnFalse()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var stockPile = gameState.StockPile;
        var tableauPile = gameState.TableauPiles[0];
        tableauPile.Cards.Add(new Card(Suit.Spades, Rank.Ace, true));
        var card = tableauPile.TopCard!;
        var move = new SingleCardMove(tableauPile.Index, stockPile.Index, card);

        // Act
        var result = move.IsValid(gameState);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SingleCardMove_Execute_ValidMove_ShouldMoveCard()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var foundationPile = gameState.FoundationPiles.First(p => p.Suit == Suit.Hearts);
        var tableauPile = gameState.TableauPiles[0];
        tableauPile.Cards.Add(new Card(Suit.Hearts, Rank.Ace, true));
        var card = tableauPile.TopCard!;
        var move = new SingleCardMove(tableauPile.Index, foundationPile.Index, card);

        // Act
        gameState.ExecuteMove(move);

        // Assert
        Assert.That(tableauPile.Cards.Count, Is.EqualTo(0));
        Assert.That(foundationPile.TopCard, Is.EqualTo(card));
    }

    [Test]
    public void SingleCardMove_Execute_InvalidMove_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var foundationPile = gameState.FoundationPiles.First(p => p.Suit == Suit.Hearts);
        var tableauPile = gameState.TableauPiles[0];
        tableauPile.Cards.Add(new Card(Suit.Spades, Rank.Ace, true));
        var card = tableauPile.TopCard!;
        var move = new SingleCardMove(tableauPile.Index, foundationPile.Index, card);

        // Act & Assert
        Assert.That(() => gameState.ExecuteMove(move), Throws.InvalidOperationException);
    }

    [Test]
    public void SingleCardMove_ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var gameState = new SolitaireGameState();
        var card = new Card(Suit.Hearts, Rank.Ace, true);
        var fromPile = gameState.TableauPiles[0];
        var toPile = gameState.FoundationPiles.First(p => p.Suit == Suit.Hearts);
        fromPile.Cards.Add(card);
        var move = new SingleCardMove(fromPile.Index, toPile.Index, card);

        // Act
        var result = move.ToString();

        // Assert
        Assert.That(result, Is.EqualTo($"Move {card} from Tableau[1] to Foundation[1]"));
    }
}
