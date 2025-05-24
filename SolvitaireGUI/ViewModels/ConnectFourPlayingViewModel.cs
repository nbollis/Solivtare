using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;
namespace SolvitaireGUI;

public enum PlayerType { Human, Agent }

public class ConnectFourPlayingViewModel : BaseViewModel
{
    #region Agent Handling

    private CancellationTokenSource? _agentCts;
    private PlayerType _player1Type = PlayerType.Agent;
    public PlayerType Player1Type
    {
        get => _player1Type;
        set
        {
            if (_player1Type != value)
            {
                _player1Type = value;
                OnPropertyChanged(nameof(Player1Type));
            }
        }
    }

    private PlayerType _player2Type = PlayerType.Agent;
    public PlayerType Player2Type
    {
        get => _player2Type;
        set
        {
            if (_player2Type != value)
            {
                _player2Type = value;
                OnPropertyChanged(nameof(Player2Type));
            }
        }
    }

    private IAgent<ConnectFourGameState, ConnectFourMove>? _player1Agent;
    public IAgent<ConnectFourGameState, ConnectFourMove>? Player1Agent
    {
        get => _player1Agent;
        set
        {
            if (_player1Agent != value)
            {
                _player1Agent = value;
                OnPropertyChanged(nameof(Player1Agent));
            }
        }
    }

    private IAgent<ConnectFourGameState, ConnectFourMove>? _player2Agent;
    public IAgent<ConnectFourGameState, ConnectFourMove>? Player2Agent
    {
        get => _player2Agent;
        set
        {
            if (_player2Agent != value)
            {
                _player2Agent = value;
                OnPropertyChanged(nameof(Player2Agent));
            }
        }
    }

    public ObservableCollection<IAgent<ConnectFourGameState, ConnectFourMove>> AvailableAgents { get; } // Populate with agent instances


    public ICommand SwapPlayersCommand { get; }
    public ICommand SwitchPlayer1ToHumanCommand { get; }
    public ICommand SwitchPlayer2ToHumanCommand { get; }
    public ICommand SwitchPlayer1ToAgentCommand { get; }
    public ICommand SwitchPlayer2ToAgentCommand { get; }
    private void SwapPlayers()
    {
        (Player1Type, Player2Type) = (Player2Type, Player1Type);
        (Player1Agent, Player2Agent) = (Player2Agent, Player1Agent);
    }

    private void StopAgentPlay()
    {
        _agentCts?.Cancel();
    }


    #endregion

    #region Gamestate Interactions

    private int _hoveredColumnIndex = -1;
    public int HoveredColumnIndex
    {
        get => _hoveredColumnIndex;
        set { _hoveredColumnIndex = value; OnPropertyChanged(nameof(HoveredColumnIndex)); }
    }

    public ConnectFourGameStateViewModel GameStateViewModel { get; }

    #endregion

    #region Game Interactions with Agents

    /// <summary>
    /// Handles the human player's move. This method is called when a human player clicks on a column to make a move.
    /// </summary>
    /// <param name="column"></param>
    private void MakeHumanMove(int column)
    {
        if (GameStateViewModel.GameState.IsGameWon || GameStateViewModel.GameState.IsGameDraw)
            return;

        int currentPlayer = GameStateViewModel.GameState.CurrentPlayer;
        if ((currentPlayer == 1 && Player1Type != PlayerType.Human) ||
            (currentPlayer == 2 && Player2Type != PlayerType.Human))
            return; // Not human's turn

        var move = new ConnectFourMove(column);
        if (GameStateViewModel.GameState.GetLegalMoves().Any(m => m.Column == column))
        {
            GameStateViewModel.ApplyMove(move);
            if (GameStateViewModel.GameState is { IsGameWon: false, IsGameDraw: false })
                StartAgentTurnIfNeeded();
        }
    }

    /// <summary>
    /// Starts the agent's turn if it's their turn. This is called recursively until a human player is encountered or the game ends.
    /// </summary>
    private void StartAgentTurnIfNeeded()
    {
        var currentPlayer = GameStateViewModel.GameState.CurrentPlayer;
        if ((currentPlayer == 1 && Player1Type == PlayerType.Agent && Player1Agent != null))
        {
            var move = Player1Agent.GetNextAction(GameStateViewModel.GameState);
            GameStateViewModel.ApplyMove(move);
            // If next turn is also an agent, continue
            if (!GameStateViewModel.GameState.IsGameWon && !GameStateViewModel.GameState.IsGameDraw)
                StartAgentTurnIfNeeded();
        }
        else if (currentPlayer == 2 && Player2Type == PlayerType.Agent && Player2Agent != null)
        {
            var move = Player2Agent.GetNextAction(GameStateViewModel.GameState);
            GameStateViewModel.ApplyMove(move);
            if (!GameStateViewModel.GameState.IsGameWon && !GameStateViewModel.GameState.IsGameDraw)
                StartAgentTurnIfNeeded();
        }
        // else: wait for human input
    }

    /// <summary>
    /// Starts the agent play loop. This will run until the game is won, drawn, or the agent is stopped.
    /// </summary>
    private async void StartAgentPlay()
    {
        _agentCts = new CancellationTokenSource();
        while (!GameStateViewModel.GameState.IsGameWon && !GameStateViewModel.GameState.IsGameDraw && !_agentCts.IsCancellationRequested)
        {
            StartAgentTurnIfNeeded();
            await Task.Delay(100);
            // If it's a human's turn, break the loop
            var currentPlayer = GameStateViewModel.GameState.CurrentPlayer;
            if ((currentPlayer == 1 && Player1Type == PlayerType.Human) ||
                (currentPlayer == 2 && Player2Type == PlayerType.Human))
                break;
        }
    }



    #endregion

    



    public ICommand MakeMoveCommand { get; }
    public ICommand StartAgentPlayCommand { get; }
    public ICommand StopAgentPlayCommand { get; }
    public ICommand ResetGameCommand { get; }

    public ConnectFourPlayingViewModel()
    {
        var gameState = new ConnectFourGameState();

        // Gameplay
        GameStateViewModel = new ConnectFourGameStateViewModel(gameState);
        MakeMoveCommand = new DelegateCommand((o) => MakeHumanMove((int)o)); // int = column
        StartAgentPlayCommand = new RelayCommand(StartAgentPlay);
        StopAgentPlayCommand = new RelayCommand(StopAgentPlay);
        ResetGameCommand = new RelayCommand(ResetGame);

        // Agents
        AvailableAgents = new ObservableCollection<IAgent<ConnectFourGameState, ConnectFourMove>>
        {
            new RandomAgent<ConnectFourGameState, ConnectFourMove>(),
            new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(new ConnectFourHeuristicEvaluator(), 5),
        };
        SwapPlayersCommand = new RelayCommand(SwapPlayers);
        SwitchPlayer1ToHumanCommand = new RelayCommand(() => Player1Type = PlayerType.Human);
        SwitchPlayer2ToHumanCommand = new RelayCommand(() => Player2Type = PlayerType.Human);
        SwitchPlayer1ToAgentCommand = new RelayCommand(() => Player1Type = PlayerType.Agent);
        SwitchPlayer2ToAgentCommand = new RelayCommand(() => Player2Type = PlayerType.Agent);
    }



    private void ResetGame()
    {
        GameStateViewModel.GameState.Reset();
        GameStateViewModel.UpdateBoard();

        // If both are agents, start play
        if (Player1Type == PlayerType.Agent && Player2Type == PlayerType.Agent)
            StartAgentPlay();
    }
}

public class ConnectFourPlayingModel : ConnectFourPlayingViewModel
{
    public static ConnectFourPlayingModel Instance => new ConnectFourPlayingModel();

    public ConnectFourPlayingModel() : base() { }
}
