using SolvitaireCore;

namespace Test.Games.Solitaire;

[TestFixture]
public class MultiCardMoveTests
{
    [Test]
    public void MultiCardMove_ValidMove_TableauToTableau()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        var from = new TableauPile(0, new List<Card>
              {
                  new Card(Suit.Hearts, Rank.Queen),
                  new Card(Suit.Spades, Rank.Jack),
                  new Card(Suit.Hearts, Rank.Ten)
              });
        var to = new TableauPile(1, new List<Card> { new Card(Suit.Clubs, Rank.King) });
        gameState.TableauPiles[0] = from;
        gameState.TableauPiles[1] = to;

        var cards = from.Cards.ToList();
        var move = new MultiCardMove(from.Index, to.Index, cards);

        // Validate
        var result = move.IsValid(gameState);

        // Assert  
        Assert.That(result, Is.True);

        // Execute  
        gameState.ExecuteMove(move);

        // Assert  
        Assert.That(from.Cards.Count, Is.EqualTo(0));
        Assert.That(to.Cards.Count, Is.EqualTo(4));
        Assert.That(to.TopCard, Is.EqualTo(cards.Last()));

        gameState.UndoMove(move);

        // Assert
        Assert.That(from.Cards.Count, Is.EqualTo(3));
        Assert.That(from.TopCard, Is.EqualTo(cards.Last()));
        Assert.That(to.Cards.Count, Is.EqualTo(1));
        Assert.That(to.TopCard, Is.EqualTo(new Card(Suit.Clubs, Rank.King)));
    }

    [Test]
    public void MultiCardMove_InvalidMove_RecycleToStock()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        var from = new WastePile(12, new List<Card>
              {
                  new Card(Suit.Hearts, Rank.Queen),
                  new Card(Suit.Spades, Rank.Jack),
                  new Card(Suit.Hearts, Rank.Ten)
              });
        var to = new StockPile(11, new List<Card> { new Card(Suit.Clubs, Rank.King) });
        gameState.WastePile = from;
        gameState.StockPile = to;


        var cards = from.Cards.ToList();
        var move = new MultiCardMove(from.Index, to.Index, cards);

        // Validate
        var result = move.IsValid(gameState);

        // Assert  
        Assert.That(result, Is.False);
        Assert.That(() => gameState.ExecuteMove(move), Throws.InvalidOperationException);
    }

    [Test]
    public void MultiCardMove_ValidMove_RecycleToStock()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        var waste = new WastePile(12, new List<Card>
              {
                  new Card(Suit.Hearts, Rank.Queen),
                  new Card(Suit.Spades, Rank.Jack),
                  new Card(Suit.Hearts, Rank.Ten)
              });

        var stock = new StockPile();
        gameState.WastePile = waste;
        gameState.StockPile = stock;

        var cards = waste.Cards.ToList();
        var wasteTop = waste.TopCard;
        var wasteBottom = waste.BottomCard;

        var move = new MultiCardMove(waste.Index, stock.Index, cards);

        // Validate
        var result = move.IsValid(gameState);

        // Assert  
        Assert.That(result, Is.True);

        // Execute
        gameState.ExecuteMove(move);

        // Assert
        Assert.That(waste.Cards.Count, Is.EqualTo(0));
        Assert.That(stock.Cards.Count, Is.EqualTo(3));
        Assert.That(stock.TopCard, Is.EqualTo(wasteBottom));
        Assert.That(stock.BottomCard, Is.EqualTo(wasteTop));

        foreach (var card in stock.Cards)
        {
            Assert.That(card.IsFaceUp, Is.False);
        }

        gameState.UndoMove(move);

        // Assert
        Assert.That(waste.Cards.Count, Is.EqualTo(3));
        Assert.That(waste.TopCard, Is.EqualTo(wasteTop));
        Assert.That(waste.BottomCard, Is.EqualTo(wasteBottom));
        Assert.That(stock.Cards.Count, Is.EqualTo(0));
        Assert.That(stock.TopCard, Is.Null);
        Assert.That(stock.BottomCard, Is.Null);
        foreach (var card in waste.Cards)
        {
            Assert.That(card.IsFaceUp, Is.True);
        }
    }

    [Test]
    public void MultiCardMove_ValidMove_CycleToWaste()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        var stock = new StockPile(11, new List<Card>
              {
                  new Card(Suit.Hearts, Rank.Queen),
                  new Card(Suit.Spades, Rank.Jack),
                  new Card(Suit.Hearts, Rank.Ten),
                  new Card(Suit.Diamonds, Rank.Four),
                  new Card(Suit.Clubs, Rank.Two),
              });

        var waste = new WastePile();
        gameState.WastePile = waste;
        gameState.StockPile = stock;

        var cards = stock.Cards.TakeLast(3).ToList();

        var move = new MultiCardMove(stock.Index, waste.Index, cards);

        // Validate
        var result = move.IsValid(gameState);

        // Assert  
        Assert.That(result, Is.True);

        // Execute
        gameState.ExecuteMove(move);

        // Assert
        Assert.That(stock.Cards.Count, Is.EqualTo(2));
        Assert.That(stock.TopCard, Is.EqualTo(new Card(Suit.Spades, Rank.Jack)));
        Assert.That(stock.BottomCard, Is.EqualTo(new Card(Suit.Hearts, Rank.Queen)));

        foreach (var card in stock.Cards)
        {
            Assert.That(card.IsFaceUp, Is.False);
        }

        Assert.That(waste.Cards.Count, Is.EqualTo(3));
        Assert.That(waste.TopCard, Is.EqualTo(new Card(Suit.Hearts, Rank.Ten)));
        Assert.That(waste.BottomCard, Is.EqualTo(new Card(Suit.Clubs, Rank.Two)));
        foreach (var card in waste.Cards)
        {
            Assert.That(card.IsFaceUp, Is.True);
        }

        gameState.UndoMove(move);

        // Assert
        Assert.That(stock.Cards.Count, Is.EqualTo(5));
        Assert.That(stock.TopCard, Is.EqualTo(new Card(Suit.Clubs, Rank.Two)));
        Assert.That(stock.BottomCard, Is.EqualTo(new Card(Suit.Hearts, Rank.Queen)));
        Assert.That(waste.Cards.Count, Is.EqualTo(0));
        Assert.That(waste.TopCard, Is.Null);
        Assert.That(waste.BottomCard, Is.Null);
        foreach (var card in stock.Cards)
        {
            Assert.That(card.IsFaceUp, Is.False);
        }

        foreach (var card in waste.Cards)
        {
            Assert.That(card.IsFaceUp, Is.True);
        }
    }
}
