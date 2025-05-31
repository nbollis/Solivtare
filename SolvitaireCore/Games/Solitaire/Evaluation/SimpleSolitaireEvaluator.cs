namespace SolvitaireCore;

public class SimpleSolitaireEvaluator : SolitaireEvaluator
{
    public override double EvaluateState(SolitaireGameState state, int? moveCount = null)
    {
        // Example: more cards in foundation = better
        return state.FoundationPiles.Sum(stack => stack.Count)
               + 0.1 * state.TableauPiles.Sum(pile => pile.Count(c => c.IsFaceUp));
    }
}