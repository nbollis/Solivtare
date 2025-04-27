using SolvitaireCore;

namespace Test.Solitaire
{
    namespace Pile
    {
        [TestFixture]
        public class PileTests
        {
            [Test]
            public void Pile_TopCard_EmptyPile_ShouldBeNull()
            {
                // Arrange  
                var pile = new TableauPile();

                // Act & Assert  
                Assert.That(pile.TopCard, Is.Null);
            }

            [Test]
            public void Pile_AddCard_InvalidCard_TryAddReturnsFalse()
            {
                // Arrange  
                var pile = new FoundationPile(Suit.Clubs);
                var card = new Card(Suit.Spades, Rank.Two);

                // Act & Assert  
                Assert.That(pile.TryAddCard(card), Is.False);
            }

            [Test]
            public void Pile_RemoveCard_EmptyPile_TryRemoveIsFalse()
            {
                // Arrange
                var pile = new TableauPile();
                var card = new Card(Suit.Spades, Rank.Ace);

                // Act & Assert
                Assert.That(pile.TryRemoveCard(card), Is.False);
            }

            [Test]
            public void Pile_RemoveCard_NonTopCard_TryRemoveIsFalse()
            {
                // Arrange
                var pile = new TableauPile(0, new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Queen)
                });
                var card = new Card(Suit.Spades, Rank.King);

                // Act & Assert
                Assert.That(pile.TryRemoveCard(card), Is.False);
            }

            [Test]
            public void Pile_RemoveCard_TopCard_ShouldRemoveCard()
            {
                // Arrange
                var pile = new TableauPile(0, new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Queen)
                });
                var card = new Card(Suit.Hearts, Rank.Queen);

                // Act
                pile.TryRemoveCard(card);

                // Assert
                Assert.That(pile.Cards.Count, Is.EqualTo(1));
                Assert.That(pile.TopCard, Is.EqualTo(new Card(Suit.Spades, Rank.King)));
            }

            [Test]
            public void Pile_CanRemoveCard_EmptyPile_ShouldReturnFalse()
            {
                // Arrange
                var pile = new TableauPile();
                var card = new Card(Suit.Spades, Rank.Ace);

                // Act
                var result = pile.CanRemoveCard(card);

                // Assert
                Assert.That(result, Is.False);
            }

            [Test]
            public void Pile_CanRemoveCard_NonTopCard_ShouldReturnFalse()
            {
                // Arrange
                var pile = new TableauPile(0, new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Queen)
                });
                var card = new Card(Suit.Spades, Rank.King);

                // Act
                var result = pile.CanRemoveCard(card);

                // Assert
                Assert.That(result, Is.False);
            }

            [Test]
            public void Pile_CanRemoveCard_TopCard_ShouldReturnTrue()
            {
                // Arrange
                var pile = new TableauPile(0, new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Queen)
                });
                var card = new Card(Suit.Hearts, Rank.Queen);

                // Act
                var result = pile.CanRemoveCard(card);

                // Assert
                Assert.That(result, Is.True);
            }
        }


        [TestFixture]
        public class FoundationPileTests
        {
            [Test]
            public void CanAddCard_EmptyPile_ShouldAllowAceOfSameSuit()
            {
                // Arrange  
                var pile = new FoundationPile(Suit.Hearts);
                var card = new Card(Suit.Hearts, Rank.Ace);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.True);
            }

            [Test]
            public void CanAddCard_NonEmptyPile_ShouldAllowNextRankOfSameSuit()
            {
                // Arrange  
                var pile = new FoundationPile(Suit.Hearts);
                pile.TryAddCard(new Card(Suit.Hearts, Rank.Ace));
                var card = new Card(Suit.Hearts, Rank.Two);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.True);
            }

            [Test]
            public void CanAddCard_WrongSuit_ShouldNotAllowCard()
            {
                // Arrange  
                var pile = new FoundationPile(Suit.Hearts);
                var card = new Card(Suit.Spades, Rank.Ace);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.False);
            }

            [Test]
            public void CanAddCard_WrongRank_ShouldNotAllowCard()
            {
                // Arrange  
                var pile = new FoundationPile(Suit.Hearts);
                pile.TryAddCard(new Card(Suit.Hearts, Rank.Ace));
                var card = new Card(Suit.Hearts, Rank.Three);
                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.False);
            }

            [Test]
            public void AddCards_WrongSuit_ShouldThrowInvalidOperationException()
            {
                // Arrange  
                var pile = new FoundationPile(Suit.Clubs);
                var cards = new List<Card>
                {
                    new Card(Suit.Clubs, Rank.Ace),
                    new Card(Suit.Spades, Rank.Two)
                };

                // Act & Assert  
                Assert.That(() => pile.AddCards(cards), Throws.InvalidOperationException);
            }

            [Test]
            public void AddCards_WrongRank_ShouldThrowInvalidOperationException()
            {
                // Arrange  
                var pile = new FoundationPile(Suit.Clubs);
                var cards = new List<Card>
                {
                    new Card(Suit.Clubs, Rank.Ace),
                    new Card(Suit.Clubs, Rank.Three)
                };
                // Act & Assert  
                Assert.That(() => pile.AddCards(cards), Throws.InvalidOperationException);
            }
        }

        [TestFixture]
        public class StockPileTests
        {
            [Test]
            public void StockPile_CanAddCard_ShouldAlwaysAllowCard()
            {
                // Arrange  
                var pile = new StockPile();
                var card = new Card(Suit.Clubs, Rank.Ten);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.True);
            }
        }

        [TestFixture]
        public class TableauPileTests
        {
            [Test]
            public void CanAddCard_EmptyPile_ShouldAllowKing()
            {
                // Arrange  
                var pile = new TableauPile();
                var card = new Card(Suit.Spades, Rank.King);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.True);
            }

            [Test]
            public void CanAddCard_EmptyPile_ShouldNotAllowNonKing()
            {
                // Arrange  
                var pile = new TableauPile();
                var card = new Card(Suit.Spades, Rank.Queen);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.False);
            }

            [Test]
            public void CanAddCard_NonEmptyPile_ShouldAllowOppositeColorAndLowerRank()
            {
                // Arrange  
                var pile = new TableauPile(0, [new Card(Suit.Hearts, Rank.Queen)]);
                var card = new Card(Suit.Spades, Rank.Jack);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.True);
            }

            [Test]
            public void CanAddCard_SameColor_ShouldNotAllowCard()
            {
                // Arrange  
                var pile = new TableauPile(0, [new Card(Suit.Hearts, Rank.Queen)]);
                var card = new Card(Suit.Diamonds, Rank.Jack);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.False);
            }

            [Test]
            public void AddCards_ValidCards_ShouldAddAllCards()
            {
                // Arrange  
                var pile = new TableauPile();
                var cards = new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Queen),
                    new Card(Suit.Spades, Rank.Jack)
                };

                // Act  
                pile.AddCards(cards);

                // Assert  
                Assert.That(pile.Cards.Count, Is.EqualTo(3));
                Assert.That(pile.TopCard, Is.EqualTo(cards[^1]));
            }

            [Test]
            public void AddCards_WrongSuit_ShouldThrowInvalidOperationException()
            {
                // Arrange  
                var pile = new TableauPile();
                var cards = new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Queen),
                    new Card(Suit.Diamonds, Rank.Jack)
                };
                // Act & Assert  
                Assert.That(() => pile.AddCards(cards), Throws.InvalidOperationException);
            }

            [Test]
            public void AddCards_WrongRank_TryAddShouldFail()
            {
                // Arrange  
                var pile = new TableauPile();
                var cards = new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Queen),
                    new Card(Suit.Spades, Rank.Ten)
                };
                // Act & Assert  
                Assert.That(() => pile.AddCards(cards), Throws.InvalidOperationException);
            }

            [Test]
            public void IsValidCardSet_EmptyList_ShouldReturnFalse()
            {
                // Arrange  
                var cards = new List<Card>();

                // Act  
                var result = TableauPile.IsValidCardSet(cards);

                // Assert  
                Assert.That(result, Is.False);
            }

            [Test]
            public void IsValidCardSet_SingleCard_ShouldReturnTrue()
            {
                // Arrange  
                var cards = new List<Card>
                {
                    new Card(Suit.Spades, Rank.King)
                };

                // Act  
                var result = TableauPile.IsValidCardSet(cards);

                // Assert  
                Assert.That(result, Is.True);
            }

            [Test]
            public void IsValidCardSet_ValidSequence_ShouldReturnTrue()
            {
                // Arrange  
                var cards = new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Queen),
                    new Card(Suit.Spades, Rank.Jack)
                };

                // Act  
                var result = TableauPile.IsValidCardSet(cards);

                // Assert  
                Assert.That(result, Is.True);
            }

            [Test]
            public void IsValidCardSet_InvalidColorSequence_ShouldReturnFalse()
            {
                // Arrange  
                var cards = new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Clubs, Rank.Queen),
                    new Card(Suit.Spades, Rank.Jack)
                };

                // Act  
                var result = TableauPile.IsValidCardSet(cards);

                // Assert  
                Assert.That(result, Is.False);
            }

            [Test]
            public void IsValidCardSet_InvalidRankSequence_ShouldReturnFalse()
            {
                // Arrange  
                var cards = new List<Card>
                {
                    new Card(Suit.Spades, Rank.King),
                    new Card(Suit.Hearts, Rank.Jack),
                    new Card(Suit.Spades, Rank.Ten)
                };

                // Act  
                var result = TableauPile.IsValidCardSet(cards);

                // Assert  
                Assert.That(result, Is.False);
            }
        }

        [TestFixture]
        public class WastePileTests
        {
            [Test]
            public void WastePile_CanAddCard_ShouldAlwaysAllowCard()
            {
                // Arrange  
                var pile = new WastePile();
                var card = new Card(Suit.Diamonds, Rank.Five);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.True);
            }
        }
    }
}
