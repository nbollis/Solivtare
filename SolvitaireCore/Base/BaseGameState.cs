namespace SolvitaireCore;

public abstract class BaseGameState<TMove> : IGameState<TMove>
    where TMove : IMove

{
    public virtual int MovesMade { get; set; } = 0;
    public virtual bool IsGameLost { get; protected set; } = false;
    public virtual bool IsGameWon { get; protected set; } = false;
    protected abstract IGameState<TMove> CloneInternal();
    /// <summary>
    /// Generates legal moves for the current game state.
    /// </summary>
    protected abstract List<TMove> GenerateLegalMoves();
    /// <summary>
    /// GameState specific implementation of executing a move.
    /// </summary>
    protected abstract void ExecuteMoveInternal(TMove move);
    /// <summary>
    /// GameState specific implementation of undoing a move.
    /// </summary>
    protected abstract void UndoMoveInternal(TMove move);
    /// <summary>
    /// Resets the state to a default position
    /// </summary>
    protected abstract void ResetInternal();

    #region Legal Move Generation

    /// <summary>
    /// Flag to indicate if the game state has changed since the last legal move generation.
    /// </summary>
    private bool _moveCacheIsDirty = true;
    private List<TMove>? _cachedLegalMoves = [];

    public List<TMove> GetLegalMoves()
    {
        if (!_moveCacheIsDirty && _cachedLegalMoves != null) 
            return _cachedLegalMoves;
        return _cachedLegalMoves = GenerateLegalMoves();
    }

    #endregion

    #region Move Making

    public bool TrackMoveHistory { get; set; } = false;
    protected List<TMove> MoveHistory { get; set; } = [];
    public IReadOnlyList<TMove> GetMoveHistory() => MoveHistory.AsReadOnly();
    public virtual string GetMoveHistoryString()
    {
        return string.Join(",", MoveHistory.Select(m => m));
    }

    /// <summary>
    /// Executes a move in the game state.
    /// </summary>
    public void ExecuteMove(TMove move)
    {
        // Invalidate Caches
        _moveCacheIsDirty = true; 
        _hashDirty = true;

        ExecuteMoveInternal(move);
        if (TrackMoveHistory)
            MoveHistory.Add(move);

        MovesMade++;
    }

    /// <summary>
    /// Undoes a move in the game state.
    /// </summary>
    public void UndoMove(TMove move)
    {
        // Invalidate Caches
        _moveCacheIsDirty = true;
        _hashDirty = true;

        UndoMoveInternal(move);
        if (TrackMoveHistory && MoveHistory.Count > 0)
            MoveHistory.RemoveAt(MoveHistory.Count - 1);

        MovesMade--;
    }

    #endregion

    #region State Changes, Hashing, and Cloning

    private int _cachedHash;
    private bool _hashDirty = true;

    public void Reset()
    {
        // Invalidate Caches
        _moveCacheIsDirty = true;
        _hashDirty = true;

        // Reset Game State
        MoveHistory.Clear();
        MovesMade = 0;
        ResetInternal();
    }

    public IGameState<TMove> Clone()
    {
        var clone = (BaseGameState<TMove>)CloneInternal();
        clone.MoveHistory = [.. MoveHistory];
        clone._moveCacheIsDirty = _moveCacheIsDirty;
        clone._cachedLegalMoves = _cachedLegalMoves != null
            ? [.. _cachedLegalMoves]
            : null;

        return clone;
    }

    protected abstract int GenerateHashCode();
    public override int GetHashCode()
    {
        if (_hashDirty)
        {
            _cachedHash = GenerateHashCode();
            _hashDirty = false;
        }
        return _cachedHash;
    }

    #endregion
}

