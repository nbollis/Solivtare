using SolvitaireCore;

namespace SolvitaireGUI;

public abstract class GameStateViewModel<TGameState, TMove> : BaseViewModel
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    public TGameState GameState { get; }
    public bool IsGameWon => GameState.IsGameWon;

    protected GameStateViewModel(TGameState gameState)
    {
        GameState = gameState;
    }

    public virtual void ApplyMove(TMove move)
    {
        GameState.ExecuteMove(move);
        UpdateBoard();
        OnPropertyChanged(nameof(IsGameWon));
        OnPropertyChanged(nameof(GameState));
    }

    public virtual void UndoMove(TMove move)
    {
        GameState.UndoMove(move);
        UpdateBoard();
        OnPropertyChanged(nameof(IsGameWon));
        OnPropertyChanged(nameof(GameState));
    }

    public abstract void UpdateBoard();
}