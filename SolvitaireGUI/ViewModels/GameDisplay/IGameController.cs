using System.Windows.Input;
using SolvitaireCore;

namespace SolvitaireGUI;

public interface IGameController<out TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    ICommand ResetGameCommand { get; }
    TGameState CurrentGameState { get; }
    bool IsGameActive { get; }
    int CurrentPlayer { get; }
    void ApplyMove(TMove move);
    void ApplyAgentMove(int playerNumber);
    List<TMove> GetLegalMoves();
}