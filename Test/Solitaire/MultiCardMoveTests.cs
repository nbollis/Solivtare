using SolvitaireCore;

namespace Test.Solitaire;

[TestFixture]
public class MultiCardMoveTests
{
    [Test]
    public void MultiCardMove_ValidMove_TableauToTableau()
    {
        // Arrange  
        var from = new TableauPile(0, new List<Card>
        {
            new Card(Suit.Hearts, Rank.Queen),
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Hearts, Rank.Ten)
        });
        var to = new TableauPile(1, new List<Card> { new Card(Suit.Clubs, Rank.King) });
        var cards = from.Cards.ToList();
        var move = new MultiCardMove(from, to, cards);

        // Validate
        var result = move.IsValid();

        // Assert  
        Assert.That(result, Is.True);

        // Execute  
        move.Execute();

        // Assert  
        Assert.That(from.Cards.Count, Is.EqualTo(0));
        Assert.That(to.Cards.Count, Is.EqualTo(4));
        Assert.That(to.TopCard, Is.EqualTo(cards.Last()));

        move.Undo();

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
        var from = new WastePile(new List<Card>
        {
            new Card(Suit.Hearts, Rank.Queen),
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Hearts, Rank.Ten)
        });
        var to = new StockPile(new List<Card> { new Card(Suit.Clubs, Rank.King) });
        var cards = from.Cards.ToList();
        var move = new MultiCardMove(from, to, cards);

        // Validate
        var result = move.IsValid();

        // Assert  
        Assert.That(result, Is.False);
        Assert.That(() => move.Execute(), Throws.InvalidOperationException);
    }

    [Test]
    public void MultiCardMove_ValidMove_RecycleToStock()
    {
        // Arrange  
        var waste = new WastePile(new List<Card>
        {
            new Card(Suit.Hearts, Rank.Queen),
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Hearts, Rank.Ten)
        });

        var stock = new StockPile();
        var cards = waste.Cards.ToList();
        var wasteTop = waste.TopCard;
        var wasteBottom = waste.BottomCard;

        var move = new MultiCardMove(waste, stock, cards);

        // Validate
        var result = move.IsValid();

        // Assert  
        Assert.That(result, Is.True);

        // Execute
        move.Execute();

        // Assert
        Assert.That(waste.Cards.Count, Is.EqualTo(0));
        Assert.That(stock.Cards.Count, Is.EqualTo(3));
        Assert.That(stock.TopCard, Is.EqualTo(wasteBottom));
        Assert.That(stock.BottomCard, Is.EqualTo(wasteTop));

        foreach (var card in stock.Cards)
        {
            Assert.That(card.IsFaceUp, Is.False);
        }

        move.Undo();

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
        var stock = new StockPile(new List<Card>
        {
            new Card(Suit.Hearts, Rank.Queen),
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Hearts, Rank.Ten),
            new Card(Suit.Diamonds, Rank.Four),
            new Card(Suit.Clubs, Rank.Two),
        });

        var waste = new WastePile();
        var cards = stock.Cards.TakeLast(3).ToList();

        var move = new MultiCardMove(stock, waste, cards);

        // Validate
        var result = move.IsValid();

        // Assert  
        Assert.That(result, Is.True);

        // Execute
        move.Execute();

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

        move.Undo();

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


    [Test]
    public void MultiCardMove_IsValid_InvalidMoveToTableau_ShouldReturnFalse()
    {
        // Arrange  
        var from = new TableauPile(0, new List<Card>
       {
           new Card(Suit.Hearts, Rank.Queen),
           new Card(Suit.Spades, Rank.Jack),
           new Card(Suit.Hearts, Rank.Ten)
       });
        var to = new TableauPile(1, new List<Card> { new Card(Suit.Hearts, Rank.King) });
        var cards = from.Cards.ToList();
        var move = new MultiCardMove(from, to, cards);

        // Act  
        var result = move.IsValid();

        // Assert  
        Assert.That(result, Is.False);
        Assert.That(() => move.Execute(), Throws.InvalidOperationException);
    }

    [Test]
    public void MultiCardMove_IsValid_MoveToFoundation_ShouldReturnFalse()
    {
        // Arrange  
        var foundationPile = new FoundationPile(Suit.Hearts);
        var tableauPile = new TableauPile(0, new List<Card>
       {
           new Card(Suit.Hearts, Rank.Queen),
           new Card(Suit.Spades, Rank.Jack),
           new Card(Suit.Hearts, Rank.Ten)
       });
        var cards = tableauPile.Cards.Skip(1).ToList();
        var move = new MultiCardMove(tableauPile, foundationPile, cards);

        // Act  
        var result = move.IsValid();

        // Assert  
        Assert.That(result, Is.False);
        Assert.That(() => move.Execute(), Throws.InvalidOperationException);
    }

    [Test]
    public void MultiCardMove_ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var cards = new List<Card>
        {
            new Card(Suit.Hearts, Rank.Ten),
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Clubs, Rank.Queen)
        };
        var fromPile = new StockPile(cards);
        var toPile = new WastePile();
        var move = new MultiCardMove(fromPile, toPile, cards);

        // Act
        var result = move.ToString();

        // Assert
        Assert.That(result, Is.EqualTo($"Cycle 3 Cards"));
    }
}
