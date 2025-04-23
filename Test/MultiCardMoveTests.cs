using SolivtaireCore;

namespace Test;

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
        var result = move.IsValid(new GameState());

        // Assert  
        Assert.That(result, Is.True);

        // Execute  
        move.Execute(new GameState());

        // Assert  
        Assert.That(from.Cards.Count, Is.EqualTo(0));
        Assert.That(to.Cards.Count, Is.EqualTo(4));
        Assert.That(to.TopCard, Is.EqualTo(cards.Last()));
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
        var result = move.IsValid(new GameState());

        // Assert  
        Assert.That(result, Is.False);
        Assert.That(() => move.Execute(new GameState()), Throws.InvalidOperationException);
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
        var result = move.IsValid(new GameState());

        // Assert  
        Assert.That(result, Is.True);

        // Execute
        move.Execute(new GameState());

        // Assert
        Assert.That(waste.Cards.Count, Is.EqualTo(0));
        Assert.That(stock.Cards.Count, Is.EqualTo(3));
        Assert.That(stock.TopCard, Is.EqualTo(wasteBottom));
        Assert.That(stock.BottomCard, Is.EqualTo(wasteTop));

        foreach (var card in stock.Cards)
        {
            Assert.That(card.IsFaceUp, Is.False);
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
        var result = move.IsValid(new GameState());

        // Assert  
        Assert.That(result, Is.True);

        // Execute
        move.Execute(new GameState());

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
        var result = move.IsValid(new GameState());

        // Assert  
        Assert.That(result, Is.False);
        Assert.That(() => move.Execute(new GameState()), Throws.InvalidOperationException);
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
        var result = move.IsValid(new GameState());

        // Assert  
        Assert.That(result, Is.False);
        Assert.That(() => move.Execute(new GameState()), Throws.InvalidOperationException);
    }
}
