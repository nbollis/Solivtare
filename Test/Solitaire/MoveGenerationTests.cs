using SolvitaireCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Solitaire
{
    [TestFixture]
    public class MoveGenerationTests
    {
        [Test]
        public void MoveGenerator_InitialTestPositionIsCorrect()
        {
            var deck = new StandardDeck(23);
            deck.Shuffle();

            var gameState = new SolitaireGameState();
            var moveGen = new SolitaireMoveGenerator();

            gameState.DealCards(deck);
            var moves = moveGen.GenerateMoves(gameState).ToList();

            var expectedMoves = new List<SolitaireMove>()
            {
                gameState.CycleMove,
                new SingleCardMove(3, 5, new(Suit.Clubs, Rank.Seven, true)),
                new SingleCardMove(4, 7, new(Suit.Hearts,Rank.Ace)),
                new SingleCardMove(3, 6, new(Suit.Hearts,Rank.Three))
            };

            Assert.That(moves.Count, Is.EqualTo(expectedMoves.Count));
        }
    }
}
