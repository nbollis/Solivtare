using SolivtaireCore;

namespace Test
{
    namespace Pile
    {
        [TestFixture]
        public class PileTests
        {
            [Test]
            public void Pile_TopCard_EmptyPile_ShouldThrowInvalidOperationException()
            {
                // Arrange  
                var pile = new TableauPile();

                // Act & Assert  
                Assert.That(() => { var topCard = pile.TopCard; }, Throws.InvalidOperationException);
            }

            [Test]
            public void Pile_AddCard_InvalidCard_ShouldThrowInvalidOperationException()
            {
                // Arrange  
                var pile = new FoundationPile(Suit.Clubs);
                var card = new Card(Suit.Spades, Rank.Two);

                // Act & Assert  
                Assert.That(() => pile.AddCard(card), Throws.InvalidOperationException);
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
                pile.AddCard(new Card(Suit.Hearts, Rank.Ace));
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
                pile.AddCard(new Card(Suit.Hearts, Rank.Ace));
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
                var pile = new TableauPile([new Card(Suit.Hearts, Rank.Queen)]);
                var card = new Card(Suit.Spades, Rank.Jack);

                // Act & Assert  
                Assert.That(pile.CanAddCard(card), Is.True);
            }

            [Test]
            public void CanAddCard_SameColor_ShouldNotAllowCard()
            {
                // Arrange  
                var pile = new TableauPile([new Card(Suit.Hearts, Rank.Queen)]);
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
            public void AddCards_WrongRank_ShouldThrowInvalidOperationException()
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
