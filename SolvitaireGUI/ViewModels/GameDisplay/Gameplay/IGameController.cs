using SolvitaireCore;

namespace SolvitaireGUI;

public interface IGameController<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    TGameState CurrentGameState { get; set; }
    bool IsGameActive { get; }
    int CurrentPlayer { get; }
    void ApplyMove(TMove move);
    void UndoMove();
    void ApplyAgentMove(int playerNumber);
    List<TMove> GetLegalMoves();
    bool EnableAnimations { get; set; }
}