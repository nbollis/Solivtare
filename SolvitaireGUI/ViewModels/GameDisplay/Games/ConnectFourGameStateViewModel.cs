using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGUI;

public class ConnectFourGameStateViewModel : TwoPlayerGameStateViewModel<ConnectFourGameState, ConnectFourMove>
{
    private int _hoveredColumnIndex = -1;
    public int HoveredColumnIndex
    {
        get => _hoveredColumnIndex;
        set { _hoveredColumnIndex = value; OnPropertyChanged(nameof(HoveredColumnIndex)); }
    }
    public ObservableCollection<int> FlatBoardCells { get; }
    public HashSet<int> WinningCellIndices =>
        [.. GameState.WinningCells.Select(cell => cell.Row * ConnectFourGameState.Columns + cell.Col)];

    public ConnectFourGameStateViewModel(ConnectFourGameState gameState) : base(gameState)
    {
        FlatBoardCells = new ObservableCollection<int>(
            Enumerable.Range(0, ConnectFourGameState.Rows * ConnectFourGameState.Columns)
                .Select(i => gameState.Board[i / ConnectFourGameState.Columns, i % ConnectFourGameState.Columns])
        );

        SetPlayerColor(1, Colors.Firebrick);
        SetPlayerColor(2, Colors.Gold);
    }

    public int? LastMoveRow { get; set; }
    public int? LastMoveCol { get; set; }
    public int LastMoveFlatIndex => LastMoveRow.HasValue && LastMoveCol.HasValue
        ? LastMoveRow.Value * 7 + LastMoveCol.Value
        : -1;
    public override void ApplyMove(ConnectFourMove move)
    {
        int row = GameState.GetRowIndexOnDrop(move.Column);

        // Reset first to force trigger
        LastMoveRow = null;
        LastMoveCol = null;
        OnPropertyChanged(nameof(LastMoveRow));
        OnPropertyChanged(nameof(LastMoveCol));
        OnPropertyChanged(nameof(LastMoveFlatIndex));

        base.ApplyMove(move);

        LastMoveRow = row;
        LastMoveCol = move.Column;
        OnPropertyChanged(nameof(LastMoveRow));
        OnPropertyChanged(nameof(LastMoveCol));
        OnPropertyChanged(nameof(LastMoveFlatIndex));
    }

    public override void UpdateBoard()
    {
        for (int i = 0; i < FlatBoardCells.Count; i++)
            FlatBoardCells[i] = GameState.Board[i / ConnectFourGameState.Columns, i % ConnectFourGameState.Columns];
        OnPropertyChanged(nameof(WinningCellIndices));
        OnPropertyChanged(nameof(FlatBoardCells));
    }
}

[ExcludeFromCodeCoverage]
public class ConnectFourGameStateModel : ConnectFourGameStateViewModel
{
    public static ConnectFourGameStateModel Instance { get; } = new();
    public ConnectFourGameStateModel() : base(new())
    {
    }
}
