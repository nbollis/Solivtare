using System.Security.Cryptography.X509Certificates;
using SolvitaireCore;

namespace Test.Games.Solitaire;

[TestFixture]
public class SolitaireGameStateTests
{
    [Test]
    public void GameState_IsGameWon_AllFoundationPilesComplete_ShouldReturnTrue()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        foreach (var pile in gameState.FoundationPiles)
        {
            for (int i = 1; i <= 13; i++)
            {
                pile.Cards.Add(new Card(pile.Suit, (Rank)i));
            }
        }

        // Act  
        var result = gameState.IsGameWon;

        // Assert  
        Assert.That(result, Is.True);
    }

    [Test]
    public void GameState_IsGameWon_NotAllFoundationPilesComplete_ShouldReturnFalse()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        foreach (var pile in gameState.FoundationPiles)
        {
            for (int i = 1; i <= 12; i++)
            {
                pile.Cards.Add(new Card(pile.Suit, (Rank)i));
            }
        }

        // Act  
        var result = gameState.IsGameWon;

        // Assert  
        Assert.That(result, Is.False);
    }

    [Test]
    public void GameState_Reset_ShouldClearAllPiles()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        gameState.FoundationPiles[0].Cards.Add(new Card(Suit.Hearts, Rank.Ace));
        gameState.TableauPiles[0].Cards.Add(new Card(Suit.Spades, Rank.King));
        gameState.StockPile.Cards.Add(new Card(Suit.Clubs, Rank.Queen));
        gameState.WastePile.Cards.Add(new Card(Suit.Diamonds, Rank.Jack));

        // Act  
        gameState.Reset();

        // Assert  
        Assert.That(gameState.FoundationPiles.All(pile => pile.Cards.Count == 0), Is.True);
        Assert.That(gameState.TableauPiles.All(pile => pile.Cards.Count == 0), Is.True);
        Assert.That(gameState.StockPile.Cards.Count, Is.EqualTo(0));
        Assert.That(gameState.WastePile.Cards.Count, Is.EqualTo(0));
    }

    [Test]
    public void GameState_DealCards_ShouldDistributeCardsCorrectly()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        var deck = new StandardDeck();

        // Act  
        gameState.DealCards(deck);

        // Assert  
        for (int i = 0; i < gameState.TableauPiles.Count; i++)
        {
            Assert.That(gameState.TableauPiles[i].Cards.Count, Is.EqualTo(i + 1));
            Assert.That(gameState.TableauPiles[i].TopCard.IsFaceUp, Is.True);
        }
        Assert.That(gameState.StockPile.Cards.Count, Is.EqualTo(52 - 28)); // 52 cards minus 28 dealt to tableau  
    }

    [Test]
    public void GameState_CycleMove_StockToWaste_ShouldMoveCorrectNumberOfCards()
    {
        // Arrange  
        var gameState = new SolitaireGameState(); // SetWeight CardsPerCycle to 3  
        var stockCards = new List<Card>
        {
           new Card(Suit.Hearts, Rank.Ace),
           new Card(Suit.Spades, Rank.Two),
           new Card(Suit.Diamonds, Rank.Three),
           new Card(Suit.Clubs, Rank.Four)
        };
        gameState.StockPile.AddCards(stockCards);

        // Act  
        var move = gameState.CycleMove;
        gameState.ExecuteMove(move);

        // Assert  
        Assert.That(gameState.StockPile.Cards.Count, Is.EqualTo(1));
        Assert.That(gameState.WastePile.Cards.Count, Is.EqualTo(3));
        Assert.That(gameState.WastePile.Cards, Is.EquivalentTo(stockCards.TakeLast(3)));
    }

    [Test]
    public void GameState_CycleMove_StockToWaste_LessThanCardsPerCycle_ShouldMoveAllRemainingCards()
    {
        // Arrange  
        var gameState = new SolitaireGameState(5); // SetWeight CardsPerCycle to 5  
        var stockCards = new List<Card>
       {
           new Card(Suit.Hearts, Rank.Ace),
           new Card(Suit.Spades, Rank.Two),
           new Card(Suit.Diamonds, Rank.Three)
       };
        gameState.StockPile.AddCards(stockCards);

        // Act  
        var move = gameState.CycleMove;
        gameState.ExecuteMove(move);

        // Assert  
        Assert.That(gameState.StockPile.Cards.Count, Is.EqualTo(0));
        Assert.That(gameState.WastePile.Cards.Count, Is.EqualTo(3));
        Assert.That(gameState.WastePile.Cards, Is.EquivalentTo(stockCards));
    }

    [Test]
    public void GameState_CycleMove_EmptyStock_ShouldNotMoveCards()
    {
        // Arrange  
        var gameState = new SolitaireGameState(3);

        // Act  
        var move = gameState.CycleMove;

        // Assert  
        Assert.That(() => gameState.ExecuteMove(move), Throws.InvalidOperationException);
    }

    [Test]
    public void GameState_GenerateLegalMoves_AreAllLegal()
    {
        var referenceDeck = new StandardDeck();
        for (int j = 0; j < 20; j++)
        {
            referenceDeck.Shuffle();

            var gameState = new SolitaireGameState();
            gameState.DealCards(referenceDeck);
            var moves = gameState.GetLegalMoves().ToList();

            SolitaireGameState[] gameStates = new SolitaireGameState[moves.Count];
            for (int i = 0; i < moves.Count; i++)
            {
                gameStates[i] = new SolitaireGameState();
                gameStates[i].DealCards(referenceDeck);

                Assert.That(moves[i].IsValid(gameStates[i]), Is.True);
                gameStates[i].ExecuteMove(moves[i]);
            }
        }

    }

    [Test]
    public void GameState_DealIsConsistent()
    {
        var referenceDeck = new StandardDeck();
        var referenceGameState = new SolitaireGameState();
        referenceGameState.DealCards(referenceDeck);

        for (int j = 0; j < 20; j++)
        {
            var gameState = new SolitaireGameState();
            gameState.DealCards(referenceDeck);
            Assert.That(gameState.Equals(referenceGameState));
        }
    }
}
