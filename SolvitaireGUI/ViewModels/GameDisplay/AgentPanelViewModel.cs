using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGUI;

public enum PlayerType { Agent, Human }

public class AgentPanelViewModel : BaseViewModel // TODO: Fully Generalize
{
    public string PlayerLabel { get; }

    #region Agent Selection and Setup

    private PlayerType _playerType;
    public PlayerType PlayerType
    {
        get => _playerType;
        set { _playerType = value; OnPropertyChanged(nameof(PlayerType)); }
    }
    public ObservableCollection<IAgent<ConnectFourGameState, ConnectFourMove>> AvailableAgents { get; }

    private IAgent<ConnectFourGameState, ConnectFourMove>? _selectedAgent;
    public IAgent<ConnectFourGameState, ConnectFourMove>? SelectedAgent
    {
        get => _selectedAgent;
        set
        {
            if (_selectedAgent != value)
            {
                _selectedAgent = value;
                OnPropertyChanged(nameof(SelectedAgent));
                OnPropertyChanged(nameof(MaxDepth)); // Notify MaxDepth may have changed
                OnPropertyChanged(nameof(IsSearchAgent));
            }
        }
    }

    public bool IsSearchAgent => PlayerType == PlayerType.Agent && SelectedAgent is ISearchAgent<ConnectFourGameState, ConnectFourMove>;
    public int? MaxDepth
    {
        get => SelectedAgent is ISearchAgent<ConnectFourGameState, ConnectFourMove> searchAgent
            ? searchAgent.MaxDepth
            : null;
        set
        {
            if (SelectedAgent is ISearchAgent<ConnectFourGameState, ConnectFourMove> searchAgent && value.HasValue)
            {
                searchAgent.MaxDepth = value.Value;
                OnPropertyChanged(nameof(MaxDepth));
            }
        }
    }

    #endregion

    public ICommand SwitchToHumanCommand { get; }
    public ICommand SwitchToAgentCommand { get; }
    public ICommand MakeMoveCommand { get; }
    public ICommand StartAgentCommand { get; }
    public ICommand StopAgentCommand { get; }

    public AgentPanelViewModel(
        string playerLabel,
        ObservableCollection<IAgent<ConnectFourGameState, ConnectFourMove>> availableAgents,
        Action makeMoveAction,
        Func<bool> isMyTurn,
        Func<bool> isGameActive)
    {
        PlayerLabel = playerLabel;
        AvailableAgents = availableAgents;
        SelectedAgent = availableAgents.Last();
        SwitchToHumanCommand = new RelayCommand(() => PlayerType = PlayerType.Human);
        SwitchToAgentCommand = new RelayCommand(() => PlayerType = PlayerType.Agent);
        MakeMoveCommand = new RelayCommand(makeMoveAction);
        StartAgentCommand = new RelayCommand(StartAgent);
        StopAgentCommand = new RelayCommand(StopAgent);

        _makeAgentMove = makeMoveAction;
        _isMyTurn = isMyTurn;
        _isGameActive = isGameActive;
    }

    #region Agent Play Control

    // Delegate to check if it's this agent's turn and to apply a move
    internal CancellationTokenSource? AgentCancelaCancellationTokenSource;
    private readonly Func<bool> _isMyTurn;
    private readonly Func<bool> _isGameActive;
    private readonly Action _makeAgentMove;
    public bool IsAgentRunning { get; private set; }

    public async void StartAgent()
    {
        if (AgentCancelaCancellationTokenSource != null)
            return;
        IsAgentRunning = true;
        AgentCancelaCancellationTokenSource = new CancellationTokenSource();
        var token = AgentCancelaCancellationTokenSource.Token;

        try
        {
            while (_isGameActive() && PlayerType == PlayerType.Agent && SelectedAgent != null &&
                   !token.IsCancellationRequested)
            {
                if (_isMyTurn())
                {
                    _makeAgentMove();
                }

                await Task.Delay(100, token);
                if (!_isGameActive())
                    break;
            }
        }
        catch (TaskCanceledException)
        {
            // If canceled, Agent should not continue on next game. 
            IsAgentRunning = false;
        }
        finally
        {
            // Leave agent running to true if not canceled, this allows the reset from the 
            // Game play controller (two player game view) to restart the agent loop. 
            AgentCancelaCancellationTokenSource = null;
        }
    }

    private void StopAgent()
    {
        AgentCancelaCancellationTokenSource?.Cancel();
        AgentCancelaCancellationTokenSource = null;
        IsAgentRunning = false;
    }

    #endregion
}

public class AgentPanelModel : AgentPanelViewModel
{
    public static AgentPanelModel Instance => new AgentPanelModel();

    public AgentPanelModel() : base("", [], null!, null!, null!) { }
}