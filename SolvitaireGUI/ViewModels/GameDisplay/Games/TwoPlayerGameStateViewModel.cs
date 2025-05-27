using SolvitaireCore;

namespace SolvitaireGUI;

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