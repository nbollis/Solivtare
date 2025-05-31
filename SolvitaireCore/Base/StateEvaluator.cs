namespace SolvitaireCore;

public abstract class StateEvaluator<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    protected static readonly (int dRow, int dCol)[] GridDirections = [(1, 0), (0, 1), (1, 1), (1, -1)];

    public double MaximumScore { get; protected set; } = 1000;

    /// <summary>
    /// Default implementation is to make the move and return the evaluation of the state. 
    /// </summary>
    public virtual double EvaluateMove(TGameState state, TMove move)
    {
        // Evaluate the move by executing it and then evaluating the state
        state.ExecuteMove(move);
        double score = EvaluateState(state);
        state.UndoMove(move);
        return score;
    }

    public virtual IEnumerable<(TMove Move, double MoveScore)> OrderMoves(List<TMove> moves, TGameState state, bool bestFirst)
    {
        if (moves == null || moves.Count == 0)
            return [];

        var scoredMoves = moves.Select(move => (Move: move, MoveScore: EvaluateMove(state, move)));

        return bestFirst
            ? scoredMoves.OrderByDescending(m => m.MoveScore)
            : scoredMoves.OrderBy(m => m.MoveScore);
    }

    public abstract double EvaluateState(TGameState state, int? moveCount = null);
}

public class AllEqualStateEvaluator<TGameState, TMove> : StateEvaluator<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    public override double EvaluateState(TGameState state, int? moveCount = null)
    {
        return 1;
    }
}