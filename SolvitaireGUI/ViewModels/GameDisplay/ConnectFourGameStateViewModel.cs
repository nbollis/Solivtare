using System.Collections.ObjectModel;
using SolvitaireCore.ConnectFour;
namespace SolvitaireGUI;

public class ConnectFourGameStateViewModel : BaseViewModel
{
    public ConnectFourGameState GameState { get; }
    public ObservableCollection<int> FlatBoardCells { get; }
    public HashSet<int> WinningCellIndices =>
        new HashSet<int>(GameState.WinningCells.Select(cell => cell.Row * ConnectFourGameState.Columns + cell.Col));


    public int CurrentPlayer => GameState.CurrentPlayer;
    public bool IsGameWon => GameState.IsGameWon;
    public bool IsGameDraw => GameState.IsGameDraw;
    public int? WinningPlayer => GameState.WinningPlayer;

    public ConnectFourGameStateViewModel(ConnectFourGameState gameState)
    {
        GameState = gameState;
        FlatBoardCells = new ObservableCollection<int>(
            Enumerable.Range(0, ConnectFourGameState.Rows * ConnectFourGameState.Columns)
                .Select(i => gameState.Board[i / ConnectFourGameState.Columns, i % ConnectFourGameState.Columns])
        );
    }

    public void ApplyMove(ConnectFourMove move)
    {
        GameState.ExecuteMove(move);
        UpdateBoard();
        OnPropertyChanged(nameof(CurrentPlayer));
        OnPropertyChanged(nameof(IsGameWon));
        OnPropertyChanged(nameof(IsGameDraw));
        OnPropertyChanged(nameof(WinningPlayer));
    }

    public void UndoMove(ConnectFourMove move)
    {
        GameState.UndoMove(move);
        UpdateBoard();
        OnPropertyChanged(nameof(CurrentPlayer));
        OnPropertyChanged(nameof(IsGameWon));
        OnPropertyChanged(nameof(IsGameDraw));
        OnPropertyChanged(nameof(WinningPlayer));
    }

    public void UpdateBoard()
    {
        for (int i = 0; i < FlatBoardCells.Count; i++)
            FlatBoardCells[i] = GameState.Board[i / ConnectFourGameState.Columns, i % ConnectFourGameState.Columns];
        OnPropertyChanged(nameof(WinningCellIndices));
    }
}