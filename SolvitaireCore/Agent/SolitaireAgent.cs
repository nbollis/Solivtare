namespace SolvitaireCore;

public abstract class SolitaireAgent : IAgent<SolitaireMove, SolitaireGameState>
{
    public abstract string Name { get; }
    public abstract SolitaireMove GetNextMove(SolitaireGameState gameState);

    public virtual void ResetState()
    {
        TranspositionTable.Clear();
    }

    // Transposition table for memoization where the Key is the hash of the game state and the Value is the score.
    protected readonly Dictionary<int, TranspositionTableEntry> TranspositionTable = new();
}

public class TranspositionTableEntry
{
    public double Score { get; set; } // The evaluation score of the state
    public int Depth { get; set; }   // The depth at which the state was evaluated
    public double Alpha { get; set; } // The alpha bound used during evaluation
    public double Beta { get; set; }  // The beta bound used during evaluation
}