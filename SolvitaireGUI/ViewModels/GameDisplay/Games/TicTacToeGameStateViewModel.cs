using SolvitaireCore.TicTacToe;

namespace SolvitaireGUI;
public class TicTacToeGameStateViewModel : TwoPlayerGameStateViewModel<TicTacToeGameState, TicTacToeMove>
{
    public TicTacToeGameStateViewModel(TicTacToeGameState gameState) : base(gameState)
    {
    }

    public override void UpdateBoard()
    {

    }
}

public class TicTacToeGameStateModel : TicTacToeGameStateViewModel
{
    public static TicTacToeGameStateModel Instance { get; } = new();
    public TicTacToeGameStateModel() : base(new())
    {
    }
}
