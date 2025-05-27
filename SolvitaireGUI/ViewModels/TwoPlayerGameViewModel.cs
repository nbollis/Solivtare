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
    where TAgent : class, IAgent<TGameState, TMove>
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

    public ICommand ResetGameCommand { get; }
    public ICommand UndoMoveCommand { get; set; }

    private void ResetGame()
    {
        GameStateViewModel.GameState.Reset();
        GameStateViewModel.UpdateBoard();
        Player1Panel.SelectedAgent.ResetState();
        Player2Panel.SelectedAgent.ResetState();
    }

    public void UndoMove()
    {
        if (!_previousMoves.TryPop(out var move))
            return;

        // Check if the move was made by an automatically running agent  
        var otherPlayerPanel = CurrentPlayer == 1 ? Player2Panel : Player1Panel;

        if (otherPlayerPanel.IsAgentRunning)
        {
            // Undo the agent's move and the opponent's move  
            GameStateViewModel.UndoMove(move);
            if (_previousMoves.TryPop(out var opponentMove))
            {
                GameStateViewModel.UndoMove(opponentMove);
            }
        }
        else
        {
            // Undo a single move  
            GameStateViewModel.UndoMove(move);
        }

        Player1Panel.RefreshLegalMoves();
        Player2Panel.RefreshLegalMoves();
    }

    #endregion

    #region Game Interactions with Agents

    private readonly Stack<TMove> _previousMoves = new();

    /// <summary>
    /// This method is called by agents and when a human player clicks on a column to make a move.
    /// </summary>
    public void ApplyMove(TMove move)
    {
        if (GameStateViewModel.IsGameWon || GameStateViewModel.IsGameDraw)
            return;

        GameStateViewModel.ApplyMove(move); 
        _previousMoves.Push(move);

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

    public TwoPlayerGameViewModel(TGameState gameState)
    {
        // Gameplay
        GameStateViewModel = gameState.ToTwoPlayerViewModel<TGameState, TMove>();
        ResetGameCommand = new RelayCommand(ResetGame);
        UndoMoveCommand = new RelayCommand(UndoMove);
        SwapPlayersCommand = new RelayCommand(SwapPlayers);

        // Agents
        Player1Panel = new AgentPanelViewModel<TGameState, TMove, TAgent>("Player 1", 1, new ObservableCollection<TAgent>(gameState.GetPossibleAgents<TGameState, TMove, TAgent>()), this);
        Player2Panel = new AgentPanelViewModel<TGameState, TMove, TAgent>("Player 2", 2, new ObservableCollection<TAgent>(gameState.GetPossibleAgents<TGameState, TMove, TAgent>()), this);
        Player1Panel.RefreshLegalMoves();
        Player2Panel.RefreshLegalMoves();
    }
}

public class TwoPlayerGameModel : TwoPlayerGameViewModel<ConnectFourGameState, ConnectFourMove, MinimaxAgent<ConnectFourGameState, ConnectFourMove>>
{
    public static TwoPlayerGameModel Instance => new TwoPlayerGameModel();

    public TwoPlayerGameModel() : base(new ConnectFourGameState()) { }
}