using SolvitaireCore;

namespace SolvitaireCore;

public abstract class BaseAgent<TGameState, TMove> : IAgent<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    public abstract string Name { get; }
    public abstract TMove GetNextAction(TGameState gameState);
    public abstract double EvaluateMoveWithAgent(TGameState gameState, TMove move, int? perspectivePlayer = null);

    /// <summary>
    /// Transposition table for memoization where the Key is the hash of the game state and the Value is the score.
    /// </summary>
    protected readonly Dictionary<int, TranspositionTableEntry> TranspositionTable = new();

    public virtual void ResetState()
    {
        TranspositionTable.Clear();
    }

    protected static readonly ListPool<ScoredMove> ScoredMovePool = new(8);
    protected static readonly IComparer<ScoredMove> MoveComparer = Comparer<ScoredMove>.Create((a, b) =>
    {
        // Compare by SearchScore first, then by MoveScore
        int scoreComparison = b.SearchScore.CompareTo(a.SearchScore);
        if (scoreComparison != 0) return scoreComparison;
        int depthComparison = a.WinDepth.CompareTo(b.WinDepth);
        if (depthComparison != 0) return depthComparison;
        return b.MoveScore.CompareTo(a.MoveScore);
    });
    protected record struct ScoredMove
    {
        public TMove Move { get; }
        public double MoveScore { get; }
        public double SearchScore { get; set; }
        public int WinDepth { get; set; }

        public ScoredMove(TMove move, double moveScore)
        {
            Move = move;
            MoveScore = moveScore;
            SearchScore = 0;
            WinDepth = int.MaxValue;
        }
    }
}

public class TranspositionTableEntry
{
    public double Score { get; set; } // The evaluation score of the state
    public int Depth { get; set; } // The depth at which the state was evaluated
    public double Alpha { get; set; } // The alpha bound used during evaluation
    public double Beta { get; set; } // The beta bound used during evaluation
}
