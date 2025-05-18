namespace SolvitaireCore;

public abstract class BaseAgent<TGameState, TMove> : IAgent<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    public abstract string Name { get; }
    public abstract TMove GetNextAction(TGameState gameState);

    /// <summary>
    /// Transposition table for memoization where the Key is the hash of the game state and the Value is the score.
    /// </summary>
    protected readonly Dictionary<int, TranspositionTableEntry> TranspositionTable = new();

    public virtual void ResetState()
    {
        TranspositionTable.Clear();
    }
}

public class TranspositionTableEntry
{
    public double Score { get; set; } // The evaluation score of the state
    public int Depth { get; set; }   // The depth at which the state was evaluated
    public double Alpha { get; set; } // The alpha bound used during evaluation
    public double Beta { get; set; }  // The beta bound used during evaluation
}