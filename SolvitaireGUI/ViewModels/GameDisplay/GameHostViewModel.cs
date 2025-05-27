using System.Collections.ObjectModel;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGUI;

public enum GameType { ConnectFour, TicTacToe, Solitaire }

public class GameHostViewModel : BaseViewModel
{
    public ObservableCollection<GameType> AvailableGames { get; } = new(Enum.GetValues<GameType>());

    private GameType _selectedGame;
    public GameType SelectedGame
    {
        get => _selectedGame;
        set
        {
            if (_selectedGame != value)
            {
                _selectedGame = value;
                OnPropertyChanged(nameof(SelectedGame));
                LoadGame(_selectedGame);
            }
        }
    }

    private object? _currentGameViewModel;
    public object? GameControllerViewModel
    {
        get => _currentGameViewModel;
        set { _currentGameViewModel = value; OnPropertyChanged(nameof(GameControllerViewModel)); }
    }

    public GameHostViewModel()
    {
        SelectedGame = AvailableGames.First();
        LoadGame(SelectedGame);
    }

    private void LoadGame(GameType type)
    {
        switch (type)
        {
            case GameType.ConnectFour:
                var gameState = new ConnectFourGameState();
                GameControllerViewModel =
                    new TwoPlayerGameViewModel<ConnectFourGameState, ConnectFourMove,
                        IAgent<ConnectFourGameState, ConnectFourMove>>(gameState);
                break;
            case GameType.TicTacToe:
                GameControllerViewModel =
                    new TwoPlayerGameViewModel<TicTacToeGameState, TicTacToeMove,
                        IAgent<TicTacToeGameState, TicTacToeMove>>(new TicTacToeGameState());
                break;

            case GameType.Solitaire:
                // Assuming SolitaireGameState and its view model are implemented
                GameControllerViewModel = new OnePlayerCardGameViewModel<SolitaireGameState, SolitaireMove, IAgent<SolitaireGameState, SolitaireMove>>(new SolitaireGameStateViewModel(new SolitaireGameState()));
                break;

        }
    }
}

public class GameHostModel : GameHostViewModel
{
    public static GameHostModel Instance { get; } = new GameHostModel();

    private GameHostModel()
    {
        // Initialize any additional properties or logic if needed  
    }
}
