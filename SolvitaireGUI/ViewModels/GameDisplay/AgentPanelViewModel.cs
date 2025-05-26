using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGUI;

public enum PlayerType { Agent, Human }

public class AgentPanelViewModel<TGameState, TMove, TAgent> : BaseViewModel
    where TGameState : IGameState<TMove>
    where TMove : IMove
    where TAgent : IAgent<TGameState, TMove>
{
    private readonly IGameController<TGameState, TMove> _controller;
    private readonly int _playerNumber;

    public string PlayerLabel { get; }

    #region Agent Selection and Setup

    private PlayerType _playerType;
    public PlayerType PlayerType
    {
        get => _playerType;
        set { _playerType = value; OnPropertyChanged(nameof(PlayerType)); OnPropertyChanged(nameof(IsSearchAgent)); }
    }
    public ObservableCollection<TAgent> AvailableAgents { get; }

    private TAgent _selectedAgent;
    public TAgent SelectedAgent
    {
        get => _selectedAgent;
        set
        {
            _selectedAgent = value;
            if (_selectedAgent is ISearchAgent<TGameState, TMove> searchAgent)
            {
                Evaluator = searchAgent.Evaluator;
                MaxDepth = searchAgent.MaxDepth;
            }
            else
            {
                Evaluator = new AllEqualStateEvaluator<TGameState, TMove>();
            }

            OnPropertyChanged(nameof(SelectedAgent));
            OnPropertyChanged(nameof(MaxDepth)); // Notify MaxDepth may have changed
            OnPropertyChanged(nameof(IsSearchAgent));
        }
    }

    public bool IsSearchAgent => PlayerType == PlayerType.Agent && SelectedAgent is ISearchAgent<TGameState, TMove>;
    public int? MaxDepth
    {
        get => SelectedAgent is ISearchAgent<TGameState, TMove> searchAgent
            ? searchAgent.MaxDepth
            : null;
        set
        {
            if (SelectedAgent is ISearchAgent<TGameState, TMove> searchAgent && value.HasValue)
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
    public ICommand MakeSpecificMoveCommand { get; }
    public ICommand StartAgentCommand { get; }
    public ICommand StopAgentCommand { get; }

    public AgentPanelViewModel(
        string playerLabel, 
        int playerNumber,
        ObservableCollection<TAgent> availableAgents,
        IGameController<TGameState, TMove> controller)
    {
        _playerNumber = playerNumber;
        _controller = controller;

        PlayerLabel = playerLabel;
        AvailableAgents = availableAgents;
        SelectedAgent = availableAgents.Last();
        PlayerType = PlayerType.Agent; // Default to agent
        if (SelectedAgent is ISearchAgent<TGameState, TMove> searchAgent)
        {
            Evaluator = searchAgent.Evaluator;
            MaxDepth = searchAgent.MaxDepth;
        }
        else
        {
            Evaluator = new AllEqualStateEvaluator<TGameState, TMove>();
        }

        SwitchToHumanCommand = new RelayCommand(() => PlayerType = PlayerType.Human);
        SwitchToAgentCommand = new RelayCommand(() => PlayerType = PlayerType.Agent);
        MakeMoveCommand = new RelayCommand(MakeAgentMove);
        StartAgentCommand = new RelayCommand(StartAgent);
        StopAgentCommand = new RelayCommand(StopAgent);
        MakeSpecificMoveCommand = new RelayCommand(MakeSpecificMove);

        OnPropertyChanged(nameof(IsAgentRunning));
    }

    #region Agent Play Control

    // Delegate to check if it's this agent's turn and to apply a move
    internal CancellationTokenSource? AgentRunningCancellationTokenSource;

    public bool CanStartAgent => !IsAgentRunning;
    public bool CanStopAgent => IsAgentRunning;

    private bool _isAgentRunning;
    public bool IsAgentRunning
    {
        get => _isAgentRunning;
        private set
        {
            if (_isAgentRunning != value)
            {
                _isAgentRunning = value;
                OnPropertyChanged(nameof(IsAgentRunning));
                OnPropertyChanged(nameof(CanStartAgent));
                OnPropertyChanged(nameof(CanStopAgent));
            }
        }
    }

    public async void StartAgent()
    {
        if (AgentRunningCancellationTokenSource != null)
            return;
        IsAgentRunning = true;
        AgentRunningCancellationTokenSource = new CancellationTokenSource();
        var token = AgentRunningCancellationTokenSource.Token;

        try
        {
            while (_controller.IsGameActive && PlayerType == PlayerType.Agent && SelectedAgent != null &&
                   !token.IsCancellationRequested)
            {
                if (_controller.CurrentPlayer == _playerNumber)
                {
                    MakeAgentMove();
                }

                await Task.Delay(100, token);
                if (!_controller.IsGameActive)
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
            AgentRunningCancellationTokenSource = null;
        }
    }

    private void StopAgent()
    {
        AgentRunningCancellationTokenSource?.Cancel();
        AgentRunningCancellationTokenSource = null;
        IsAgentRunning = false;
    }

    private void MakeAgentMove()
    {
        _controller.ApplyAgentMove(_playerNumber);
        MovesMade = _controller.CurrentGameState.MovesMade;
        RefreshLegalMoves();
    }

    private void MakeSpecificMove()
    {
        if (SelectedMove != null && _controller.CurrentPlayer == _playerNumber)
        {
            _controller.ApplyMove(SelectedMove.Move);
            MovesMade = _controller.CurrentGameState.MovesMade;
            RefreshLegalMoves();
        }
    }

    #endregion

    #region Gui Display

    protected StateEvaluator<TGameState, TMove> Evaluator;
    private MoveViewModel<TMove>? _selectedMove;
    public MoveViewModel<TMove>? SelectedMove
    {
        get => _selectedMove;
        set
        {
            _selectedMove = value;
            OnPropertyChanged(nameof(SelectedMove));
        }
    }

    private double _evaluation;
    public double Evaluation
    {
        get => _evaluation;
        set
        {
            _evaluation = value;
            OnPropertyChanged(nameof(Evaluation));
        }
    }

    private int _movesMade;
    public int MovesMade
    {
        get => _movesMade;
        set
        {
            _movesMade = value;
            OnPropertyChanged(nameof(MovesMade));
        }
    }

    public ObservableCollection<MoveViewModel<TMove>> LegalMoves { get; } = new();
    public void RefreshLegalMoves()
    {
        LegalMoves.Clear();
        if (!_controller.IsGameActive)
            return;

        var clone = (TGameState)_controller.CurrentGameState.Clone();

        foreach (var move in _controller.GetLegalMoves())
        {
            clone.ExecuteMove(move);
            double eval = Evaluator.EvaluateState(clone, _playerNumber);
            clone.UndoMove(move);

            LegalMoves.Add(new MoveViewModel<TMove>(move, eval));
        }
    }

    #endregion
}

public class AgentPanelModel : AgentPanelViewModel<ConnectFourGameState, ConnectFourMove, MinimaxAgent<ConnectFourGameState, ConnectFourMove>>
{
    public static AgentPanelModel Instance => new AgentPanelModel();

    public AgentPanelModel() : base("", 1, [], null!) { }
}