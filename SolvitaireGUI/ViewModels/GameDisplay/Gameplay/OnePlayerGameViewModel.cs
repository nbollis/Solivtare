using System.Windows.Input;
using SolvitaireCore;

namespace SolvitaireGUI;

public class OnePlayerCardGameViewModel<TGameState, TMove, TAgent> : OnePlayerGameViewModel<TGameState, TMove, TAgent>
where TGameState : ICardGameState, IGameState<TMove>, new()
where TMove : IMove
where TAgent : class, IAgent<TGameState, TMove>
{
    private readonly StandardDeck _deck;

    public override ICommand ResetGameCommand { get; }
    public override ICommand NewGameCommand { get; }
    public OnePlayerCardGameViewModel(TGameState gameState) : this(gameState.ToViewModel<TGameState, TMove>()) { }
    public OnePlayerCardGameViewModel(GameStateViewModel<TGameState, TMove> gameStateViewModel) : base(gameStateViewModel)
    {
        _deck ??= new ObservableStandardDeck(23);
        _deck.Shuffle();

        GameStateViewModel.GameState.DealCards(_deck);
        ShadowGameState = (TGameState)CurrentGameState.Clone();
        GameStateViewModel.UpdateBoard();
        AgentPanel.RefreshLegalMoves();

        NewGameCommand = new RelayCommand(NewGame);
        ResetGameCommand = new RelayCommand(ResetGame);
        ResetGame();
    }

    protected override void ResetGame()
    {
        PreviousMoves.Clear();
        _deck.FlipAllCardsDown();

        var newGameState = new TGameState();
        newGameState.DealCards(_deck);

        GameStateViewModel = newGameState.ToViewModel<TGameState, TMove>();
        ShadowGameState = (TGameState)CurrentGameState.Clone();
        GameStateViewModel.UpdateBoard();

        OnPropertyChanged(nameof(GameStateViewModel));
        AgentPanel.RefreshLegalMoves();
    }

    private void NewGame()
    {
        _deck.Shuffle();
        ResetGame();
    }
}

public class OnePlayerGameViewModel<TGameState, TMove, TAgent> : BaseViewModel, IGameController<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
    where TAgent : class, IAgent<TGameState, TMove>
{
    protected TGameState ShadowGameState { get; set; }
    public AgentPanelViewModel<TGameState, TMove, TAgent> AgentPanel { get; }
    public GameStateViewModel<TGameState, TMove> GameStateViewModel { get; set; }

    public virtual ICommand ResetGameCommand { get; }
    public virtual ICommand NewGameCommand { get; }
    public ICommand UndoMoveCommand { get; }

    // IGameController implementation
    public TGameState CurrentGameState => GameStateViewModel.GameState;
    public bool IsGameActive => !GameStateViewModel.IsGameWon;
    public int CurrentPlayer => 1; // Single player

    public OnePlayerGameViewModel(TGameState gameState) : this(gameState.ToViewModel<TGameState, TMove>()) { }

    public OnePlayerGameViewModel(GameStateViewModel<TGameState, TMove> gameStateViewModel)
    {
        GameStateViewModel = gameStateViewModel;
        ShadowGameState = (TGameState)CurrentGameState.Clone();
        var possibleAgents = gameStateViewModel.GameState.GetPossibleAgents<TGameState, TMove, TAgent>();
        AgentPanel = new AgentPanelViewModel<TGameState, TMove, TAgent>("Agent", 1, [.. possibleAgents], this);

        ResetGameCommand = new RelayCommand(ResetGame);
        NewGameCommand = new RelayCommand(ResetGame);
        UndoMoveCommand = new RelayCommand(UndoMove);

        AgentPanel.RefreshLegalMoves();
    }

    public List<TMove> GetLegalMoves() =>
        GameStateViewModel.GameState.GetLegalMoves()
            .Where(p => !p.IsTerminatingMove).ToList(); // Temporary bypassing of the skip game move

    protected readonly Stack<TMove> PreviousMoves = new();
    public void ApplyMove(TMove move)
    {
        if (GameStateViewModel.GameState.IsGameWon)
            return;
        GameStateViewModel.ApplyMove(move);
        ShadowGameState.ExecuteMove(move);
        AgentPanel.RefreshLegalMoves();
        PreviousMoves.Push(move);
    }

    public void UndoMove()
    {
        if (PreviousMoves.Count == 0)
            return;
        var move = PreviousMoves.Pop();
        GameStateViewModel.UndoMove(move);
        ShadowGameState.UndoMove(move);
        AgentPanel.RefreshLegalMoves();
    }

    public void ApplyAgentMove(int playerNumber)
    {
        if (playerNumber != 1)
            return;

        // TODO: Handle terminating action
        var move = AgentPanel.SelectedAgent.GetNextAction(ShadowGameState);
        ApplyMove(move);
    }

    protected virtual void ResetGame()
    {
        GameStateViewModel.GameState.Reset();
        ShadowGameState = (TGameState)GameStateViewModel.GameState.Clone();
        GameStateViewModel.UpdateBoard();
        AgentPanel.RefreshLegalMoves();
    }
}

public class OnePlayerGameModel : OnePlayerGameViewModel<SolitaireGameState, SolitaireMove, IAgent<SolitaireGameState, SolitaireMove>>
{
    public static OnePlayerGameModel Instance => new OnePlayerGameModel();

    public OnePlayerGameModel() : base(new SolitaireGameStateModel()) { }
}
