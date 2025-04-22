using SolivtareCore;

namespace Test
{
    [TestFixture]
    public class CardTests
    {
        [Test]
        public void Card_Equality_SameSuitAndRank_ShouldBeEqual()
        {
            // Arrange  
            var card1 = new Card(Suit.Hearts, Rank.Ace);
            var card2 = new Card(Suit.Hearts, Rank.Ace);

            // Act & Assert  
            Assert.That(card1, Is.EqualTo(card2));
        }

        [Test]
        public void Card_Equality_DifferentSuit_ShouldNotBeEqual()
        {
            // Arrange  
            var card1 = new Card(Suit.Hearts, Rank.Ace);
            var card2 = new Card(Suit.Spades, Rank.Ace);

            // Act & Assert  
            Assert.That(card1, Is.Not.EqualTo(card2));
        }

        [Test]
        public void Card_Equality_DifferentRank_ShouldNotBeEqual()
        {
            // Arrange  
            var card1 = new Card(Suit.Hearts, Rank.Ace);
            var card2 = new Card(Suit.Hearts, Rank.King);

            // Act & Assert  
            Assert.That(card1, Is.Not.EqualTo(card2));
        }

        [Test]
        public void Card_HashCode_SameSuitAndRank_ShouldBeEqual()
        {
            // Arrange  
            var card1 = new Card(Suit.Clubs, Rank.Ten);
            var card2 = new Card(Suit.Clubs, Rank.Ten);

            // Act & Assert  
            Assert.That(card1.GetHashCode(), Is.EqualTo(card2.GetHashCode()));
        }

        [Test]
        public void Card_HashCode_DifferentSuitOrRank_ShouldNotBeEqual()
        {
            // Arrange  
            var card1 = new Card(Suit.Diamonds, Rank.Queen);
            var card2 = new Card(Suit.Spades, Rank.Queen);
            var card3 = new Card(Suit.Diamonds, Rank.King);

            // Act & Assert  
            Assert.That(card1.GetHashCode(), Is.Not.EqualTo(card2.GetHashCode()));
            Assert.That(card1.GetHashCode(), Is.Not.EqualTo(card3.GetHashCode()));
        }

        [Test]
        public void Card_IsFaceUp_DefaultValue_ShouldBeFalse()
        {
            // Arrange  
            var card = new Card(Suit.Spades, Rank.Jack);

            // Act & Assert  
            Assert.That(card.IsFaceUp, Is.False);
        }

        [Test]
        public void Card_IsFaceUp_SetToTrue_ShouldBeTrue()
        {
            // Arrange  
            var card = new Card(Suit.Spades, Rank.Jack);

            // Act  
            card.IsFaceUp = true;

            // Assert  
            Assert.That(card.IsFaceUp, Is.True);
        }
    }
}