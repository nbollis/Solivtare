using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireGuiFunctions;

namespace SolvitaireGUI;

public class AgentPlayingViewModel : BaseViewModel
{
    private readonly Stack<SolitaireMove> _previousMoves;
    private GameStateViewModel _gameStateViewModel;
    private readonly StandardDeck _deck;
    public GameStateViewModel GameStateViewModel
    {
        get => _gameStateViewModel;
        set
        {
            _gameStateViewModel = value;
            OnPropertyChanged(nameof(GameStateViewModel));
        }
    }

    public AgentPlayingViewModel()
    {
        _evaluator = new SecondSolitaireEvaluator();
        _previousMoves = new();
        _deck ??= new ObservableStandardDeck(23);
        _deck.Shuffle();

        var gameState = new SolitaireGameState();
        gameState.DealCards(_deck);
        GameStateViewModel = new GameStateViewModel(gameState);
        LegalMoves = new();
        Agent = new RandomAgent();
        AllAgents = new()
        {
            Agent,
            new BruteForceEvaluationAgent(_evaluator),
            new AlphaBetaEvaluationAgent(_evaluator),
        };

        _shadowGameState = GameStateViewModel.BaseGameState.Clone();

        ResetGameCommand = new RelayCommand(ResetGame);
        MakeMoveCommand = new RelayCommand(AgentMakeMove);
        UndoMoveCommand = new RelayCommand(UndoMove);
        NewGameCommand = new RelayCommand(NewGame);
        MakeSpecificMoveCommand = new DelegateCommand(MakeSpecificMove);
        StartAgentCommand = new RelayCommand(StartAgent);
        StopAgentCommand = new RelayCommand(StopAgent);
        Refresh();
    }


    #region Agent Playing

    private SolitaireEvaluator _evaluator;
    private SolitaireGameState _shadowGameState; // Used during gameplay to not update UI while moves are made and unmade
    private CancellationTokenSource? _agentCancellationTokenSource;
    private SolitaireAgent _agent;
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

    public SolitaireAgent Agent
    {
        get => _agent;
        set
        {
            _agent = value;
            OnPropertyChanged(nameof(Agent));
        }
    }

    public ObservableCollection<SolitaireAgent> AllAgents { get; set; }

    public ICommand StartAgentCommand { get; set; }
    public ICommand StopAgentCommand { get; set; }
    public ICommand MakeMoveCommand { get; set; }

    private async void StartAgent()
    {
        if (_agentCancellationTokenSource != null)
        {
            // Agent is already running
            return;
        }

        _agentCancellationTokenSource = new CancellationTokenSource();
        var token = _agentCancellationTokenSource.Token;

        try
        {
            
            while (!GameStateViewModel.IsGameWon && !token.IsCancellationRequested)
            {
                // Check if the game is unwinnable
                if (Agent.IsGameUnwinnable(_shadowGameState))
                {
                    // Terminate the current game and start a new one
                    //StopAgent();
                    NewGame();
                }

                // Use the shadow game state for the agent's search
                var move = await Task.Run(() => Agent.GetNextMove(_shadowGameState), token);

                // Apply the move to the real game state and update the shadow state
                GameStateViewModel.ApplyMove(move);
                _shadowGameState.ExecuteMove(move);
                _previousMoves.Push(move);
                Refresh();

                await Task.Delay(100, token); // Optional: Add a small delay for better UI responsiveness
            }
        }
        catch (TaskCanceledException)
        {
            // Task was canceled, handle if necessary
        }
        finally
        {
            _agentCancellationTokenSource = null;
        }
    }

    private void StopAgent()
    {
        if (_agentCancellationTokenSource != null)
        {
            _agentCancellationTokenSource.Cancel();
            _agentCancellationTokenSource = null;
        }
    }

    private void AgentMakeMove()
    {
        // Use the shadow game state for the agent's search
        var move = Agent.GetNextMove(_shadowGameState);

        // Apply the move to the real game state and update the shadow state
        GameStateViewModel.ApplyMove(move);
        _shadowGameState.ExecuteMove(move);
        _previousMoves.Push(move);
        Refresh();
    }

    #endregion

    #region Human Interaction

    private MoveViewModel _selectedMove;
    public MoveViewModel SelectedMove
    {
        get => _selectedMove;
        set
        {
            _selectedMove = value;
            OnPropertyChanged(nameof(SelectedMove));
        }
    }

    private ObservableCollection<MoveViewModel> _legalMoves;
    public ObservableCollection<MoveViewModel> LegalMoves
    {
        get => _legalMoves;
        set
        {
            _legalMoves = value;
            OnPropertyChanged(nameof(LegalMoves));
        }
    }

    public ICommand MakeSpecificMoveCommand { get; set; }
    public ICommand UndoMoveCommand { get; set; }
    private void MakeSpecificMove(object? moveObject)
    {
        SolitaireMove move;
        switch (moveObject)
        {
            case MoveViewModel vm:
                move = vm.Move;
                break;
            case SolitaireMove mo:
                move = mo;
                break;
            default:
                return;
        }

        GameStateViewModel.ApplyMove(move);
        _shadowGameState.ExecuteMove(move);
        _previousMoves.Push(move);
        Refresh();
    }

    private void UndoMove()
    {
        if (!_previousMoves.TryPop(out var move))
            return;
        GameStateViewModel.UndoMove(move);
        _shadowGameState.UndoMove(move);
        Refresh();
    }

    #endregion

    #region Play Setup

    public ICommand ResetGameCommand { get; set; }
    public ICommand NewGameCommand { get; set; }
    private void ResetGame()
    {
        _previousMoves.Clear();
        _deck.FlipAllCardsDown();
        var gameState = new SolitaireGameState();
        gameState.DealCards(_deck!);
        Agent.ResetState();

        GameStateViewModel = new(gameState);
        _shadowGameState = gameState.Clone(); // Sync the shadow state
        Refresh();
    }

    private void NewGame()
    {
        _deck.Shuffle();
        ResetGame();
    }

    #endregion

    public void Refresh()
    {
        LegalMoves.Clear();
        foreach (var move in GameStateViewModel.GetLegalMoves())
        {
            _shadowGameState.ExecuteMove(move);
            double eval = _evaluator.Evaluate(_shadowGameState);
            _shadowGameState.UndoMove(move);
            LegalMoves.Add(new MoveViewModel(move, eval));
        }

        Evaluation = _evaluator.Evaluate(_shadowGameState);
        OnPropertyChanged(nameof(GameStateViewModel));
        OnPropertyChanged(nameof(Agent));
    }
}

public class AgentPlayingModel : AgentPlayingViewModel
{
    public static AgentPlayingModel Instance => new();

    AgentPlayingModel()
    {
        Agent = new RandomAgent();
    }
}