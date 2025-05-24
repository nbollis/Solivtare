using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGUI;

public class AgentPanelViewModel : BaseViewModel // TODO: Fully Generalize
{
    public string PlayerLabel { get; }

    private PlayerType _playerType;
    public PlayerType PlayerType
    {
        get => _playerType;
        set { _playerType = value; OnPropertyChanged(nameof(PlayerType)); }
    }

    private IAgent<ConnectFourGameState, ConnectFourMove>? _selectedAgent;
    public IAgent<ConnectFourGameState, ConnectFourMove>? SelectedAgent
    {
        get => _selectedAgent;
        set { _selectedAgent = value; OnPropertyChanged(nameof(SelectedAgent)); }
    }

    public ObservableCollection<IAgent<ConnectFourGameState, ConnectFourMove>> AvailableAgents { get; }

    public ICommand SwitchToHumanCommand { get; }
    public ICommand SwitchToAgentCommand { get; }
    public ICommand MakeMoveCommand { get; }

    public AgentPanelViewModel(
        string playerLabel,
        ObservableCollection<IAgent<ConnectFourGameState, ConnectFourMove>> availableAgents,
        Action makeMoveAction)
    {
        PlayerLabel = playerLabel;
        AvailableAgents = availableAgents;
        SwitchToHumanCommand = new RelayCommand(() => PlayerType = PlayerType.Human);
        SwitchToAgentCommand = new RelayCommand(() => PlayerType = PlayerType.Agent);
        MakeMoveCommand = new RelayCommand(() => makeMoveAction());
    }
}