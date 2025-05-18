namespace SolvitaireCore;

public interface IGameState
{
    public bool IsGameLost { get; }
    public bool IsGameWon { get; }
    void Reset();
}

public interface IGameState<TMove> : IGameState where TMove : IMove
{
    List<TMove> GetLegalMoves();
    void ExecuteMove(TMove move);
    void UndoMove(TMove move);

    IGameState<TMove> Clone();
}