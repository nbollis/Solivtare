using SolvitaireCore;

namespace SolvitaireGUI;

public abstract class GameStateViewModel<TGameState, TMove> : BaseViewModel
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    public IGameController<TGameState, TMove>? GameController { get; }
    public TGameState GameState { get; }
    public bool IsGameWon => GameState.IsGameWon;
    public bool EnableAnimations => GameController?.EnableAnimations ?? true;

    protected GameStateViewModel(TGameState gameState, IGameController<TGameState, TMove>? controller)
    {
        GameState = gameState;
        GameController= controller;
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