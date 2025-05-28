using System.Windows.Media;
using SolvitaireCore;
using Color = System.Windows.Media.Color;

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


    #region Player Colors
    public Color Player1Color { get; set; } = Colors.Red;
    public Color Player2Color { get; set; } = Colors.Yellow;

    public void SetPlayerColor(int player, Color color)
    {
        if (player == 1)
            Player1Color = color;
        else if (player == 2)
            Player2Color = color;
        OnPropertyChanged(nameof(Player1Color));
        OnPropertyChanged(nameof(Player2Color));
    }

    #endregion
}