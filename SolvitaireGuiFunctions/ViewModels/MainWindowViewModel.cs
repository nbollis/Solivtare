using SolvitaireCore;

namespace SolvitaireGuiFunctions;

public class MainWindowViewModel : BaseViewModel
{
    private GameStateViewModel _gameStateViewModel;

    public GameStateViewModel GameStateViewModel
    {
        get => _gameStateViewModel;
        set 
        { 
            _gameStateViewModel = value;
            OnPropertyChanged(nameof(GameStateViewModel));
            _gameStateViewModel.Refresh();
        }
    }

    public MainWindowViewModel()
    {
        var gameState = new GameState();
        var deck = new StandardDeck();
        deck.Shuffle();
        gameState.DealCards(deck);
        GameStateViewModel = new GameStateViewModel(gameState);
    }
}