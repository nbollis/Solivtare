using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;
namespace SolvitaireGUI;

// TODO: Generalize this to a two player game view model. 
public class ConnectFourPlayingViewModel : BaseViewModel
{
    #region Agent Handling

    private CancellationTokenSource? _agentCts;
    public AgentPanelViewModel Player1Panel { get; } 
    public AgentPanelViewModel Player2Panel { get; }

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
        }
    }

    #endregion

    public ICommand MakeMoveCommand { get; }
    public ICommand ResetGameCommand { get; }

    public ConnectFourPlayingViewModel()
    {
        var gameState = new ConnectFourGameState();

        // Gameplay
        GameStateViewModel = new ConnectFourGameStateViewModel(gameState);
        MakeMoveCommand = new DelegateCommand((o) => MakeHumanMove((int)o)); // int = column
        ResetGameCommand = new RelayCommand(ResetGame);

        // Agents
        var availableAgents = new ObservableCollection<IAgent<ConnectFourGameState, ConnectFourMove>>
        {
            new RandomAgent<ConnectFourGameState, ConnectFourMove>(),
            new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(new ConnectFourHeuristicEvaluator(), 5),
        };

        Player1Panel = new AgentPanelViewModel(
            "Player 1",
            availableAgents,
            () => MakeAgentMove(1),
            () => GameStateViewModel.GameState.CurrentPlayer == 1,
            () => !GameStateViewModel.GameState.IsGameWon && !GameStateViewModel.GameState.IsGameDraw
        );

        Player2Panel = new AgentPanelViewModel(
            "Player 2",
            availableAgents,
            () => MakeAgentMove(2),
            () => GameStateViewModel.GameState.CurrentPlayer == 2,
            () => !GameStateViewModel.GameState.IsGameWon && !GameStateViewModel.GameState.IsGameDraw
        );

        SwapPlayersCommand = new RelayCommand(SwapPlayers);
    }

    private void ResetGame()
    {
        GameStateViewModel.GameState.Reset();
        GameStateViewModel.UpdateBoard();

        // Restart agent auto-play if it was running before reset
        if (Player1Panel.IsAgentRunning)
            Player1Panel.StartAgentCommand.Execute(null);
        if (Player2Panel.IsAgentRunning)
            Player2Panel.StartAgentCommand.Execute(null);
    }
}

public class ConnectFourPlayingModel : ConnectFourPlayingViewModel
{
    public static ConnectFourPlayingModel Instance => new ConnectFourPlayingModel();

    public ConnectFourPlayingModel() : base() { }
}
