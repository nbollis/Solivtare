using SolvitaireCore;

namespace SolvitaireGuiFunctions;

public class MainWindowViewModel : BaseViewModel
{
    private AgentPlayingViewModel _agentPlayingViewModel;

    public AgentPlayingViewModel AgentPlayingViewModel
    {
        get => _agentPlayingViewModel;
        set 
        { 
            _agentPlayingViewModel = value;
            OnPropertyChanged(nameof(AgentPlayingViewModel));
            _agentPlayingViewModel.Refresh();
        }
    }

    public MainWindowViewModel()
    {
        var gameState = new GameState();
        var deck = new StandardDeck();
        deck.Shuffle();
        gameState.DealCards(deck);
        AgentPlayingViewModel = new AgentPlayingViewModel();
        AgentPlayingViewModel.GameStateViewModel.GameState = gameState;
    }
}