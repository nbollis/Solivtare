namespace SolvitaireCore;

public abstract class SolitaireEvaluator : StateEvaluator<SolitaireGameState, SolitaireMove>
{
    public override double EvaluateMove(SolitaireGameState state, SolitaireMove move)
    {
        if (move.IsTerminatingMove)
        {
            // Reuse skip evaluation logic
            return EvaluateSkipScore(state);
        }

        double score = 0;

        // to foundation
        if (move.ToPileIndex == SolitaireGameState.FoundationStartIndex) score += 5;

        // tableau to tableau
        if (move.ToPileIndex <= SolitaireGameState.TableauEndIndex && move.FromPileIndex > SolitaireGameState.TableauEndIndex) score += 2;

        // cycle move
        if (move.FromPileIndex == SolitaireGameState.StockIndex) score += 1;

        // pull from waste
        if (move.FromPileIndex == SolitaireGameState.WasteIndex) score += 3;

        return score;
    }

    // Do not skip to game by default. Override this method in derived class to change the behavior. 
    public virtual double EvaluateSkipScore(SolitaireGameState state) => -10;
}