namespace SolvitaireCore;

public class SimpleSolitaireEvaluator : SolitaireEvaluator
{
    public override double Evaluate(SolitaireGameState state)
    {
        // Example: more cards in foundation = better
        return state.FoundationPiles.Sum(stack => stack.Count)
               + 0.1 * state.TableauPiles.Sum(pile => pile.Count(c => c.IsFaceUp));
    }
}

public class SecondSolitaireEvaluator : SolitaireEvaluator
{
    public override double Evaluate(SolitaireGameState state)
    {
        double score = 0;

        // Add one for every card in the foundation piles. 
        score += 0.5 * state.FoundationPiles.Sum(pile => pile.Count);

        foreach (var tableau in state.TableauPiles)
        {
            foreach (var card in tableau)
            {
                // reward face up and punish face down cards in tableau
                if (card.IsFaceUp)
                    score += 0.1;
                else
                    score -= 0.1;

                // Really punish an ace in tableau
                if (card.Rank is Rank.Ace)
                    score -= 1;
            }
        }

        return score;
    }
}