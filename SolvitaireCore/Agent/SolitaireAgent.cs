namespace SolvitaireCore;

public abstract class SolitaireAgent : IAgent<SolitaireGameState>
{
    public abstract string Name { get; }
    public abstract AgentDecision GetNextAction(SolitaireGameState gameState);

    /// <summary>
    /// Transposition table for memoization where the Key is the hash of the game state and the Value is the score.
    /// </summary>
    protected readonly Dictionary<int, TranspositionTableEntry> TranspositionTable = new();

    public virtual void ResetState()
    {
        TranspositionTable.Clear();
    }

    /// <summary>
    /// Determines if the current game state is unwinnable.
    /// </summary>
    /// <param name="gameState">The current game state.</param>
    /// <returns>True if the game is unwinnable, otherwise false.</returns>
    public virtual bool IsGameUnwinnable(SolitaireGameState gameState)
    {
        // Default implementation: Check if there are no legal moves and the game is not won.
        return false;
    }
}

public class TranspositionTableEntry
{
    public double Score { get; set; } // The evaluation score of the state
    public int Depth { get; set; }   // The depth at which the state was evaluated
    public double Alpha { get; set; } // The alpha bound used during evaluation
    public double Beta { get; set; }  // The beta bound used during evaluation
}