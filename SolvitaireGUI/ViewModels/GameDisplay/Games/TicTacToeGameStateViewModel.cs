using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using SolvitaireCore.TicTacToe;

namespace SolvitaireGUI;
public class TicTacToeGameStateViewModel : TwoPlayerGameStateViewModel<TicTacToeGameState, TicTacToeMove>
{
    public int BoardSize => TicTacToeGameState.Size;
    public int BoardCellCount => BoardSize * BoardSize;

    private int _hoveredColumnIndex = -1;
    public int HoveredColumnIndex
    {
        get => _hoveredColumnIndex;
        set { _hoveredColumnIndex = value; OnPropertyChanged(nameof(HoveredColumnIndex)); }
    }

    private int _hoveredRowIndex = -1;
    public int HoveredRowIndex
    {
        get => _hoveredRowIndex;
        set { _hoveredRowIndex = value; OnPropertyChanged(nameof(HoveredRowIndex)); }
    }

    public ObservableCollection<int> FlatBoardCells { get; } = new(
        Enumerable.Range(0, TicTacToeGameState.Size * TicTacToeGameState.Size).Select(_ => 0));
    public HashSet<int> WinningCellIndices =>
        [..GameState.WinningCells.Select(cell => cell.Row * TicTacToeGameState.Size + cell.Col)];

    public TicTacToeGameStateViewModel(TicTacToeGameState gameState) : base(gameState)
    {
        for (int i = 0; i < TicTacToeGameState.Size * TicTacToeGameState.Size; i++)
            FlatBoardCells[i] = gameState.Board[i / TicTacToeGameState.Size, i % TicTacToeGameState.Size];
        SetPlayerColor(1, Colors.Maroon);
        SetPlayerColor(2, Colors.BlueViolet);
    }

    public override void UpdateBoard()
    {
        for (int i = 0; i < FlatBoardCells.Count; i++)
            FlatBoardCells[i] = GameState.Board[i / TicTacToeGameState.Size, i % TicTacToeGameState.Size];
        OnPropertyChanged(nameof(WinningCellIndices));
        OnPropertyChanged(nameof(FlatBoardCells));
    }
}

[ExcludeFromCodeCoverage]
public class TicTacToeGameStateModel : TicTacToeGameStateViewModel
{
    public static TicTacToeGameStateModel Instance { get; } = new();
    public TicTacToeGameStateModel() : base(new())
    {
        var move = new TicTacToeMove(1, 1);
        ApplyMove(move);
        move = new TicTacToeMove(0, 0);
        ApplyMove(move);
        move = new TicTacToeMove(2, 2);
        ApplyMove(move);
        move = new TicTacToeMove(0, 1);
        ApplyMove(move);
    }
}
