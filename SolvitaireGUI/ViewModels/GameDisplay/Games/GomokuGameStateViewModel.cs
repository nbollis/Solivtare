using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using SolvitaireCore.Gomoku;

namespace SolvitaireGUI;

public class GomokuGameStateViewModel : TwoPlayerGameStateViewModel<GomokuGameState, GomokuMove>
{
    private int _boardSize;
    public int BoardSize
    {
        get => _boardSize;
        set
        {
            if (_boardSize == value) return; 

            _boardSize = value;
            ResetBoard(_boardSize);
            OnPropertyChanged(nameof(BoardSize));
            OnPropertyChanged(nameof(BoardCellCount));
        }
    }

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

    public ObservableCollection<int> FlatBoardCells { get; private set; }
    public HashSet<int> WinningCellIndices =>
        [..GameState.WinningCells.Select(cell => cell.Row * BoardSize + cell.Col)];

    public GomokuGameStateViewModel(GomokuGameState gameState, IGameController<GomokuGameState, GomokuMove>? controller = null)
        : base(gameState, controller)
    {
        FlatBoardCells = new ObservableCollection<int>(
            Enumerable.Range(0, BoardCellCount).Select(i => gameState.Board[i / BoardSize, i % BoardSize])
        );
        SetPlayerColor(1, Colors.Black);
        SetPlayerColor(2, Colors.White);
    }

    public override void UpdateBoard()
    {
        for (int i = 0; i < FlatBoardCells.Count; i++)
            FlatBoardCells[i] = GameState.Board[i / BoardSize, i % BoardSize];
        OnPropertyChanged(nameof(WinningCellIndices));
        OnPropertyChanged(nameof(FlatBoardCells));
    }

    public void ResetBoard(int newSize)
    {
        GameState = new GomokuGameState(newSize);
        _boardSize = newSize;
        FlatBoardCells = new ObservableCollection<int>(
            Enumerable.Range(0, BoardCellCount).Select(i => 0)
        );
        OnPropertyChanged(nameof(FlatBoardCells));
        UpdateBoard();
    }
}

[ExcludeFromCodeCoverage]
public class GomokuGameStateModel : GomokuGameStateViewModel
{
    public static GomokuGameStateModel Instance { get; } = new();
    public GomokuGameStateModel() : base(new GomokuGameState(), null!) { }
}
