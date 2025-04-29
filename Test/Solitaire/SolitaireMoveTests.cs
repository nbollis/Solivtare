using SolvitaireCore;
using SolvitaireIO;

namespace Test.Solitaire
{
    [TestFixture]
    public class SolitaireMoveTests
    {
        private SolitaireGameState _gameState;
        private StandardDeck _deck;

        [SetUp]
        public void SetUp()
        {
            _deck = new StandardDeck();
            _deck.Shuffle();
            _gameState = new SolitaireGameState();
            _gameState.DealCards(_deck);
            var temp = GameStateSerializer.Serialize(_gameState);
        }

        [Test]
        public void TestWasteToFoundationMove()
        {
            // Arrange
            var wastePile = _gameState.WastePile;
            wastePile.Cards.Add(new(Suit.Clubs, Rank.Ace, true));
            var foundationPile = _gameState.FoundationPiles.FirstOrDefault(f => f.CanAddCard(wastePile.TopCard!));
            Assume.That(foundationPile, Is.Not.Null, "No valid foundation pile for the top card of the waste pile.");

            var move = new SingleCardMove(SolitaireGameState.WasteIndex, foundationPile!.Index, wastePile.TopCard!);

            // Act
            var originalFaceUp = wastePile.TopCard!.IsFaceUp;
            _gameState.ExecuteMove(move);
            var afterMoveFaceUp = foundationPile.TopCard!.IsFaceUp;
            _gameState.UndoMove(move);
            var afterUndoFaceUp = wastePile.TopCard!.IsFaceUp;

            // Assert
            Assert.That(originalFaceUp, Is.True, "Waste pile top card should be face up before the move.");
            Assert.That(afterMoveFaceUp, Is.True, "Foundation pile top card should be face up after the move.");
            Assert.That(afterUndoFaceUp, Is.True, "Waste pile top card should be face up after undoing the move.");
        }

        [Test]
        public void TestWasteToTableauMove()
        {
            // Arrange
            var wastePile = _gameState.WastePile;
            wastePile.Cards.Add(new(Suit.Hearts, Rank.Nine, true));
            var tableauPile = _gameState.TableauPiles.FirstOrDefault(t => t.CanAddCard(wastePile.TopCard!));
            Assume.That(tableauPile, Is.Not.Null, "No valid tableau pile for the top card of the waste pile.");

            var move = new SingleCardMove(SolitaireGameState.WasteIndex, tableauPile!.Index, wastePile.TopCard!);

            // Act
            var originalFaceUp = wastePile.TopCard!.IsFaceUp;
            _gameState.ExecuteMove(move);
            var afterMoveFaceUp = tableauPile.TopCard!.IsFaceUp;
            _gameState.UndoMove(move);
            var afterUndoFaceUp = wastePile.TopCard!.IsFaceUp;

            // Assert
            Assert.That(originalFaceUp, Is.True, "Waste pile top card should be face up before the move.");
            Assert.That(afterMoveFaceUp, Is.True, "Tableau pile top card should be face up after the move.");
            Assert.That(afterUndoFaceUp, Is.True, "Waste pile top card should be face up after undoing the move.");
        }

        [Test]
        public void TestTableauToFoundationMove()
        {
            // Arrange
            var foundation = _gameState.FoundationPiles.First(p => p.Suit == Suit.Spades);
            foundation.TryAddCard(new (Suit.Spades, Rank.Ace, true));
            foundation.TryAddCard(new (Suit.Spades, Rank.Two, true));
            foundation.TryAddCard(new (Suit.Spades, Rank.Three, true));

            var tableauPile = _gameState.TableauPiles[2];
            Assume.That(tableauPile, Is.Not.Null, "No valid tableau pile for a move to the foundation.");

            var foundationPile = _gameState.FoundationPiles.First(f => f.CanAddCard(tableauPile!.TopCard!));
            var move = new SingleCardMove(tableauPile!.Index, foundationPile.Index, tableauPile.TopCard!);

            // Act
            var originalFaceUp = tableauPile.TopCard!.IsFaceUp;
            var originalUnderneathFaceUp = tableauPile.Cards.SkipLast(1).Last().IsFaceUp;
            _gameState.ExecuteMove(move);
            var afterMoveFaceUp = foundationPile.TopCard!.IsFaceUp;
            var afterMoveUnderneathFaceUp = tableauPile.Cards.SkipLast(1).Last().IsFaceUp;
            _gameState.UndoMove(move);
            var afterUndoFaceUp = tableauPile.TopCard!.IsFaceUp;
            var afterUndoUnderneathIsFaceUp = tableauPile.Cards.SkipLast(1).Last().IsFaceUp;

            // Assert
            Assert.That(originalFaceUp, Is.True, "Tableau pile top card should be face up before the move.");
            Assert.That(afterMoveFaceUp, Is.True, "Foundation pile top card should be face up after the move.");
            Assert.That(afterUndoFaceUp, Is.True, "Tableau pile top card should be face up after undoing the move.");
            Assert.That(originalUnderneathFaceUp, Is.False, "Tableau pile second card should be face down before the move.");
            Assert.That(afterMoveUnderneathFaceUp, Is.False, "Foundation pile second card should be face down after the move.");
            Assert.That(afterUndoUnderneathIsFaceUp, Is.False, "Tableau pile second card should be face down after undoing the move.");
        }

        [Test]
        public void TestTableauToTableauMove()
        {
            // Arrange
            var card = new Card(Suit.Hearts, Rank.Nine, true);
            var fromTableau = _gameState.TableauPiles[3];
            fromTableau.TryAddCard(card); 
            Assume.That(fromTableau, Is.Not.Null, "No valid tableau pile to move from.");


            var toTableau = _gameState.TableauPiles[4];
            Assume.That(toTableau, Is.Not.Null, "No valid tableau pile to move to.");

            var move = new MultiCardMove(fromTableau.Index, toTableau!.Index, [card]);

            // Act
            var originalFaceUp = fromTableau.TopCard!.IsFaceUp;
            var originalUnderneathFaceUp = fromTableau.Cards.SkipLast(1).Last().IsFaceUp;
            _gameState.ExecuteMove(move);
            var afterMoveFaceUp = fromTableau.TopCard!.IsFaceUp;
            var afterMoveUnderneathFaceUp = fromTableau.Cards.SkipLast(1).Last().IsFaceUp;
            _gameState.UndoMove(move);
            var afterUndoFaceUp = fromTableau.TopCard!.IsFaceUp;
            var afterUndoUnderneathIsFaceUp = fromTableau.Cards.SkipLast(1).Last().IsFaceUp;

            // Assert
            Assert.That(originalFaceUp, Is.True, "From tableau pile top card should be face up before the move.");
            Assert.That(afterMoveFaceUp, Is.True, "To tableau pile top card should be face up after the move.");
            Assert.That(afterUndoFaceUp, Is.True, "From tableau pile top card should be face up after undoing the move.");
            Assert.That(originalUnderneathFaceUp, Is.True, "Tableau pile second card should be face down before the move.");
            Assert.That(afterMoveUnderneathFaceUp, Is.False, "Tableau pile second card should be face down after the move.");
            Assert.That(afterUndoUnderneathIsFaceUp, Is.True, "Tableau pile second card should be face down after undoing the move.");
        }

        [Test]
        public void TestStockToWasteMove()
        {
            // Arrange
            var move = _gameState.CycleMove;

            // Act
            var originalFaceUp = _gameState.StockPile.TopCard?.IsFaceUp ?? false;
            _gameState.ExecuteMove(move);
            var afterMoveFaceUp = _gameState.WastePile.TopCard!.IsFaceUp;
            _gameState.UndoMove(move);
            var afterUndoFaceUp = _gameState.StockPile.TopCard?.IsFaceUp ?? false;

            // Assert
            Assert.That(originalFaceUp, Is.False, "Stock pile top card should be face down before the move.");
            Assert.That(afterMoveFaceUp, Is.True, "Waste pile top card should be face up after the move.");
            Assert.That(afterUndoFaceUp, Is.False, "Stock pile top card should be face down after undoing the move.");
        }

        [Test]
        public void TestWasteToStockRecycleMove()
        {
            while (_gameState.StockPile.Count > 0)
            {
                _gameState.ExecuteMove(_gameState.CycleMove);
            }

            // Arrange
            Assume.That(_gameState.StockPile.IsEmpty, Is.True, "Stock pile must be empty for a recycle move.");
            Assume.That(_gameState.WastePile.IsEmpty, Is.False, "Waste pile must not be empty for a recycle move.");

            var move = new MultiCardMove(SolitaireGameState.WasteIndex, SolitaireGameState.StockIndex, _gameState.WastePile.Cards);

            // Act
            var originalFaceUp = _gameState.WastePile.TopCard!.IsFaceUp;
            _gameState.ExecuteMove(move);
            var afterMoveFaceUp = _gameState.StockPile.TopCard!.IsFaceUp;
            _gameState.UndoMove(move);
            var afterUndoFaceUp = _gameState.WastePile.TopCard!.IsFaceUp;

            // Assert
            Assert.That(originalFaceUp, Is.True, "Waste pile top card should be face up before the move.");
            Assert.That(afterMoveFaceUp, Is.False, "Stock pile top card should be face down after the move.");
            Assert.That(afterUndoFaceUp, Is.True, "Waste pile top card should be face up after undoing the move.");
        }
    }
}
