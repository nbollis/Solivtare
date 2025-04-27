using static System.Formats.Asn1.AsnWriter;

namespace SolvitaireCore;

public class SecondSolitaireEvaluator : SolitaireEvaluator
{
    public override double Evaluate(SolitaireGameState state)
    {
        double score = 0;

        // Reward foundation piles 
        score += 3.2 * state.FoundationPiles.Sum(pile => pile.Count);

        // Penalize stock and waste piles
        score -= 0.02 * state.StockPile.Count;
        score -= 0.01 * state.WastePile.Count;
        score -= 0.1 * state.CycleCount;

        // Reward empty Tableaus
        score += 0.3 * state.TableauPiles.Count(pile => pile.IsEmpty);

        // Add Tableau pile
        foreach (var tableau in state.TableauPiles)
        {
            int faceDownCount = tableau.Cards.TakeWhile(card => !card.IsFaceUp).Count();
            score -= 0.75 * faceDownCount; // Penalize face-down cards

            score += 0.5 * (tableau.Count - faceDownCount); // reward face-up cards

            if (tableau.BottomCard == null) // reward empty pile
                score += 0.2;
            else if (tableau.BottomCard.IsFaceUp) // reward bottom card of pile exposed. 
            {
                score += 0.2;
                if (tableau.BottomCard.Rank == Rank.King) // extra reward that card being a king
                    score += 0.4;
            }

            int sequenceLength = 1;
            for (int i = tableau.Cards.Count - 1; i > 0; i--)
            {
                if (tableau.Cards[i].Rank == Rank.Ace)
                    score -= 1; // punish ace in tableau

                // Check if the current card is a valid continuation of the sequence
                if (tableau.Cards[i].Color != tableau.Cards[i - 1].Color &&
                    tableau.Cards[i].Rank == tableau.Cards[i - 1].Rank - 1)
                {
                    sequenceLength++;
                }
                else
                {
                    break;
                }
            }
            score += 0.05 * sequenceLength; // Reward longer sequences
        }


        // Reward each unique move 
        var moves = state.GetLegalMoves().ToHashSet();
        score += 0.01 * moves.Count;

        return score;
    }
}