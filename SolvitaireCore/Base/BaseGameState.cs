
namespace SolvitaireCore;

public abstract class BaseGameState<TMove> : IGameState<TMove>
    where TMove : IMove

{
    public virtual int MovesMade { get; protected set; } = 0;
    public virtual bool IsGameLost { get; protected set; } = false;
    public virtual bool IsGameWon { get; protected set; } = false;

    public abstract void Reset();
    public abstract IGameState<TMove> Clone();

    protected BaseGameState()
    {
        CachedMoves = [];
    }

    // TODO: Implement move history tracking. 
    // This will likely involve taking the current public abstract methods and making the protected. 
    // The base game state then has these methods, does all shared operations, then calls and returns the result of the specific game state's protected method. 
    #region Move Handling

    public abstract List<TMove> GetLegalMoves();
    public abstract void ExecuteMove(TMove move);
    public abstract void UndoMove(TMove move);

    // cached moves to avoid recalculating them
    protected bool MoveCacheIsDirty { get; set; }
    protected List<TMove>? CachedMoves { get; set; }

    #endregion
}

