namespace SolvitaireCore;

public interface IGameState<TMove>
{
    bool IsGameLost { get; }
    bool IsGameWon { get; }
    void Reset();
    List<TMove> GetLegalMoves();
    void ExecuteMove(TMove move);
    void UndoMove(TMove move);
}