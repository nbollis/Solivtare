namespace SolvitaireCore;

public class SecondSolitaireEvaluator : SolitaireEvaluator
{
    public override double Evaluate(SolitaireGameState state)
    {
        double score = 0;

        // Add one for every card in the foundation piles. 
        score += 0.75 * state.FoundationPiles.Sum(pile => pile.Count);

        foreach (var tableau in state.TableauPiles)
        {
            foreach (var card in tableau)
            {
                // reward face up and punish face down cards in tableau
                if (card.IsFaceUp)
                    score += 0.1;
                else if (card.Rank is Rank.Ace) // Really punish a face down ace in tableau
                    score -= 1;
                else
                    score -= 0.1;
            }
        }

        // Add a small punishment for cards in the stock pile
        score -= 0.01 * state.StockPile.Count;

        return score;
    }
}