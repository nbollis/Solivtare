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

public abstract class TwoPlayerGameStateViewModel<TGameState, TMove> : GameStateViewModel<TGameState, TMove>
    where TGameState : ITwoPlayerGameState<TMove>
    where TMove : IMove
{
    public int MovesMade => GameState.MovesMade;
    public bool IsGameActive => !(GameState.IsGameWon || GameState.IsGameDraw);
    public int CurrentPlayer => GameState.CurrentPlayer;
    public bool IsGameDraw => GameState.IsGameDraw;
    public int? WinningPlayer => GameState.WinningPlayer;

    protected TwoPlayerGameStateViewModel(TGameState gameState) : base(gameState)
    {
    }

    public override void ApplyMove(TMove move)
    {
        base.ApplyMove(move);

        OnPropertyChanged(nameof(CurrentPlayer));
        OnPropertyChanged(nameof(IsGameActive));
        OnPropertyChanged(nameof(IsGameDraw));
        OnPropertyChanged(nameof(WinningPlayer));
        OnPropertyChanged(nameof(MovesMade));
    }

    public override void UndoMove(TMove move)
    {
        base.UndoMove(move);

        OnPropertyChanged(nameof(CurrentPlayer));
        OnPropertyChanged(nameof(IsGameActive));
        OnPropertyChanged(nameof(IsGameDraw));
        OnPropertyChanged(nameof(WinningPlayer));
        OnPropertyChanged(nameof(MovesMade));
    }
}
