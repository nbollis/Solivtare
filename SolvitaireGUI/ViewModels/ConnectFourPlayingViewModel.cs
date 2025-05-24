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
    public AgentPanelViewModel Player1Panel { get; } 
    public AgentPanelViewModel Player2Panel { get; }

    public ObservableCollection<IAgent<ConnectFourGameState, ConnectFourMove>> AvailableAgents { get; } // Populate with agent instances
    public ICommand SwapPlayersCommand { get; }
    private void SwapPlayers()
    {
        (Player1Panel.PlayerType, Player2Panel.PlayerType) = (Player2Panel.PlayerType, Player1Panel.PlayerType);
        (Player1Panel.SelectedAgent, Player2Panel.SelectedAgent) = (Player2Panel.SelectedAgent, Player1Panel.SelectedAgent);
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
        if ((currentPlayer == 1 && Player1Panel.PlayerType != PlayerType.Human) ||
            (currentPlayer == 2 && Player2Panel.PlayerType != PlayerType.Human))
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
        if ((currentPlayer == 1 && Player1Panel.PlayerType == PlayerType.Agent && Player1Panel.SelectedAgent != null))
        {
            var move = Player1Panel.SelectedAgent.GetNextAction(GameStateViewModel.GameState);
            GameStateViewModel.ApplyMove(move);
            // If next turn is also an agent, continue
            if (!GameStateViewModel.GameState.IsGameWon && !GameStateViewModel.GameState.IsGameDraw)
                StartAgentTurnIfNeeded();
        }
        else if (currentPlayer == 2 && Player2Panel.PlayerType == PlayerType.Agent && Player2Panel.SelectedAgent != null)
        {
            var move = Player2Panel.SelectedAgent.GetNextAction(GameStateViewModel.GameState);
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
            if ((currentPlayer == 1 && Player1Panel.PlayerType == PlayerType.Human) ||
                (currentPlayer == 2 && Player2Panel.PlayerType == PlayerType.Human))
                break;
        }
    }

    private void MakeAgentMove(int playerNumber)
    {
        var currentPlayer = GameStateViewModel.GameState.CurrentPlayer;
        if (currentPlayer != playerNumber)
            return;

        var agent = playerNumber == 1 ? Player1Panel.SelectedAgent : Player2Panel.SelectedAgent;
        if (agent != null)
        {
            var move = agent.GetNextAction(GameStateViewModel.GameState);
            GameStateViewModel.ApplyMove(move);
            if (!GameStateViewModel.GameState.IsGameWon && !GameStateViewModel.GameState.IsGameDraw)
                StartAgentTurnIfNeeded();
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
        Player1Panel = new AgentPanelViewModel("Player 1", AvailableAgents, () => MakeAgentMove(1));
        Player2Panel = new AgentPanelViewModel("Player 2", AvailableAgents, () => MakeAgentMove(2));

        SwapPlayersCommand = new RelayCommand(SwapPlayers);
    }



    private void ResetGame()
    {
        GameStateViewModel.GameState.Reset();
        GameStateViewModel.UpdateBoard();

        // If both are agents, start play
        if (Player1Panel.PlayerType == PlayerType.Agent && Player2Panel.PlayerType == PlayerType.Agent)
            StartAgentPlay();
    }
}

public class ConnectFourPlayingModel : ConnectFourPlayingViewModel
{
    public static ConnectFourPlayingModel Instance => new ConnectFourPlayingModel();

    public ConnectFourPlayingModel() : base() { }
}
