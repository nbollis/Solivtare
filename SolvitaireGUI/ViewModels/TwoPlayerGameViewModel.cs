using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGUI;

// TODO: Generalize this to a two player game view model. 
/// <summary>
/// Class that handles interactions between agents and the game state.
/// </summary>
/// <typeparam name="TGameState"></typeparam>
/// <typeparam name="TMove"></typeparam>
/// <typeparam name="TAgent"></typeparam>
public class TwoPlayerGameViewModel<TGameState, TMove, TAgent> : BaseViewModel, IGameController<TGameState, TMove>
    where TGameState : ITwoPlayerGameState<TMove>
    where TMove : IMove
    where TAgent : IAgent<TGameState, TMove>
{
    public bool IsGameActive => GameStateViewModel.IsGameActive;
    public int CurrentPlayer => GameStateViewModel.CurrentPlayer;

    #region Agent Handling

    public AgentPanelViewModel<TGameState, TMove, TAgent> Player1Panel { get; }
    public AgentPanelViewModel<TGameState, TMove, TAgent> Player2Panel { get; }

    public ICommand SwapPlayersCommand { get; }
    private void SwapPlayers()
    {
        (Player1Panel.PlayerType, Player2Panel.PlayerType) = (Player2Panel.PlayerType, Player1Panel.PlayerType);
        (Player1Panel.SelectedAgent, Player2Panel.SelectedAgent) = (Player2Panel.SelectedAgent, Player1Panel.SelectedAgent);
    }

    #endregion

    #region Gamestate Interactions

    public TGameState CurrentGameState => GameStateViewModel.GameState;
    public TwoPlayerGameStateViewModel<TGameState, TMove> GameStateViewModel { get; }

    public List<TMove> GetLegalMoves() => GameStateViewModel.GameState.GetLegalMoves();

    #endregion

    #region Game Interactions with Agents

    /// <summary>
    /// Handles the human player's move. This method is called when a human player clicks on a column to make a move.
    /// </summary>
    public void ApplyMove(TMove move)
    {
        if (GameStateViewModel.IsGameWon || GameStateViewModel.IsGameDraw)
            return;

        GameStateViewModel.ApplyMove(move);

        Player1Panel.RefreshLegalMoves();
        Player2Panel.RefreshLegalMoves();
    }

    public void ApplyAgentMove(int playerNumber)
    {
        if (CurrentPlayer != playerNumber)
            return;

        var agent = playerNumber == 1 ? Player1Panel.SelectedAgent : Player2Panel.SelectedAgent;
        if (agent != null)
        {
            var move = agent.GetNextAction(GameStateViewModel.GameState);
            ApplyMove(move);
        }
    }

    #endregion

    public ICommand MakeMoveCommand { get; }
    public ICommand ResetGameCommand { get; }

    public TwoPlayerGameViewModel(TGameState gameState)
    {
        // Gameplay
        GameStateViewModel = gameState.ToTwoPlayerViewModel<TGameState, TMove>();
        MakeMoveCommand = new DelegateCommand((m)=>ApplyMove((TMove)m)); // int = column
        ResetGameCommand = new RelayCommand(ResetGame);

        // Agents
        var availableAgents =
            new ObservableCollection<TAgent>(gameState.GetPossibleAgents<TGameState, TMove, TAgent>());

        Player1Panel = new AgentPanelViewModel<TGameState, TMove, TAgent>("Player 1", 1, availableAgents, this);
        Player2Panel = new AgentPanelViewModel<TGameState, TMove, TAgent>("Player 2", 2, availableAgents, this);

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

public class TwoPlayerGameModel : TwoPlayerGameViewModel<ConnectFourGameState, ConnectFourMove, ConnectFourAgent>
{
    public static TwoPlayerGameModel Instance => new TwoPlayerGameModel();

    public TwoPlayerGameModel() : base(new ConnectFourGameState()) { }
}
