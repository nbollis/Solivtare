namespace SolvitaireCore;

public interface IGameState
{
    int MovesMade { get; }
    public bool IsGameLost { get; }
    public bool IsGameWon { get; }
    void Reset();
}

public interface ICardGameState : IGameState
{
    void DealCards(StandardDeck deck);
}

public interface IGameState<TMove> : IGameState where TMove : IMove
{
    List<TMove> GetLegalMoves();
    void ExecuteMove(TMove move);
    void UndoMove(TMove move);

    IGameState<TMove> Clone();
}

public interface ITwoPlayerGameState<TMove> : IGameState<TMove> where TMove : IMove
{
    public int? WinningPlayer { get; }
    int CurrentPlayer { get; }
    bool IsGameDraw { get; }
    bool IsPlayerWin(int player);
    bool IsPlayerLoss(int player);
}