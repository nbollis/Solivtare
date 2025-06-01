using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
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

    public ObservableCollection<ConnectFourCellViewModel> BoardCells { get; }

    public HashSet<int> WinningCellIndices =>
        [.. GameState.WinningCells.Select(cell => cell.Row * ConnectFourGameState.Columns + cell.Col)];
    public ConnectFourGameStateViewModel(ConnectFourGameState gameState, IGameController<ConnectFourGameState, ConnectFourMove>? controller = null) 
        : base(gameState, controller)
    {
        BoardCells = new ObservableCollection<ConnectFourCellViewModel>(
            Enumerable.Range(0, ConnectFourGameState.Rows)
                .SelectMany(row => Enumerable.Range(0, ConnectFourGameState.Columns)
                    .Select(col => new ConnectFourCellViewModel(row, col, gameState.Board[row, col])))
        );

        SetPlayerColor(1, Colors.Firebrick);
        SetPlayerColor(2, Colors.Gold);
    }


    public (int Row, int Col)? LastMove => GameState.LastMove;

    public override void ApplyMove(ConnectFourMove move)
    {
        // This updates the game state, then the board
        base.ApplyMove(move);

        // Ensure property changes are on the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            OnPropertyChanged(nameof(LastMove));
        });
        //OnPropertyChanged(nameof(LastMove));
    }

    public override void UpdateBoard()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            for (int row = 0; row < ConnectFourGameState.Rows; row++)
            for (int col = 0; col < ConnectFourGameState.Columns; col++)
                BoardCells[row * ConnectFourGameState.Columns + col].Value = GameState.Board[row, col];

            OnPropertyChanged(nameof(BoardCells));
            OnPropertyChanged(nameof(WinningCellIndices));
        });
    }
}

[ExcludeFromCodeCoverage]
public class ConnectFourGameStateModel : ConnectFourGameStateViewModel
{
    public static ConnectFourGameStateModel Instance { get; } = new();
    public ConnectFourGameStateModel() : base(new(), null!)
    {
    }
}

public class ConnectFourCellViewModel : BaseViewModel
{
    public int Row { get; }
    public int Col { get; }
    private int _value; // Backing field for Value property  
    public int Value// 0 = empty, 1 = player1, 2 = player2
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }
    public ConnectFourCellViewModel(int row, int col, int value)
    {
        Row = row;
        Col = col;
        Value = value;
    }
}