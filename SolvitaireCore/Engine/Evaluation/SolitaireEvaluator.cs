namespace SolvitaireCore;

public abstract class SolitaireEvaluator : IStateEvaluator<SolitaireGameState>
{
    protected static SolitaireMoveGenerator MoveGenerator { get; } = new();
    public abstract double Evaluate(SolitaireGameState state, int? moveCount = null);
    public virtual bool ShouldSkipGame(SolitaireGameState state)
    {
        return state.IsGameLost;
    }
}