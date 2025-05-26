using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;

namespace SolvitaireGUI;

public class AgentPlayingViewModel : BaseViewModel
{
    private readonly Stack<SolitaireMove> _previousMoves;
    private SolitaireGameStateViewModel _solitaireGameStateViewModel;
    private readonly ObservableStandardDeck _deck;
    public SolitaireGameStateViewModel SolitaireGameStateViewModel
    {
        get => _solitaireGameStateViewModel;
        set
        {
            _solitaireGameStateViewModel = value;
            OnPropertyChanged(nameof(SolitaireGameStateViewModel));
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
        SolitaireGameStateViewModel = new SolitaireGameStateViewModel(gameState);
        LegalMoves = new();
        Agent = new RandomSolitaireAgent();
        AllAgents = new()
        {
            Agent,
            new MaximizingSolitaireAgent(_evaluator, 5),
        };

        _shadowGameState = (SolitaireGameState)SolitaireGameStateViewModel.BaseGameState.Clone();

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

    private StateEvaluator<SolitaireGameState, SolitaireMove> _evaluator;
    private SolitaireGameState _shadowGameState; // Used during gameplay to not update UI while moves are made and unmade
    private CancellationTokenSource? _agentCancellationTokenSource;
    private BaseAgent<SolitaireGameState, SolitaireMove> _agent;
    private double _evaluation;
    private int _movesMade;

    public double Evaluation
    {
        get => _evaluation;
        set
        {
            _evaluation = value;
            OnPropertyChanged(nameof(Evaluation));
        }
    }

    public int MovesMade
    {
        get => _movesMade;
        set
        {
            _movesMade = value;
            OnPropertyChanged(nameof(MovesMade));
        }
    }

    public BaseAgent<SolitaireGameState, SolitaireMove> Agent
    {
        get => _agent;
        set
        {
            _agent = value;
            OnPropertyChanged(nameof(Agent));
        }
    }

    public ObservableCollection<BaseAgent<SolitaireGameState, SolitaireMove>> AllAgents { get; set; }

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
            
            while (!SolitaireGameStateViewModel.IsGameWon && !token.IsCancellationRequested)
            {
                // Use the shadow game state for the agent's search
                var decision = await Task.Run(() => Agent.GetNextAction(_shadowGameState), token);

                if (decision.IsTerminatingMove)
                {
                    // Handle the case where the agent decides to skip the game
                    break;
                }
                else if (decision != null)
                {
                    // Apply the move to the real game state and update the shadow state
                    MakeSpecificMove(decision);
                }
                else
                {
                    Console.WriteLine("❌ Invalid move or no move available.");
                    break;
                }

                

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
        var action = Agent.GetNextAction(_shadowGameState);
        if (action.IsTerminatingMove)
        {
            NewGame();
            return;
        }
        if (action != null)
        {
            var move = action;
            MakeSpecificMove(move);
        }
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
                move = vm.Move as SolitaireMove;
                break;
            case SolitaireMove { IsTerminatingMove: false } mo:
                move = mo;
                break;
            case SolitaireMove { IsTerminatingMove: true }:
                NewGame();
                return;
            default:
                return;
        }

        MovesMade++;
        SolitaireGameStateViewModel.ApplyMove(move);
        _shadowGameState.ExecuteMove(move);
        _previousMoves.Push(move);
        Refresh();
    }

    private void UndoMove()
    {
        if (!_previousMoves.TryPop(out var move))
            return;
        MovesMade--;
        SolitaireGameStateViewModel.UndoMove(move);
        _shadowGameState.UndoMove(move);
        Refresh();
    }

    #endregion

    #region Play Setup

    public ICommand ResetGameCommand { get; set; }
    public ICommand NewGameCommand { get; set; }
    private void ResetGame()
    {
        MovesMade = 0;
        _previousMoves.Clear();
        _deck.FlipAllCardsDown();
        var gameState = new SolitaireGameState();
        gameState.DealCards(_deck!);
        Agent.ResetState();

        SolitaireGameStateViewModel = new(gameState);
        _shadowGameState = (SolitaireGameState)gameState.Clone(); // Sync the shadow state
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
        foreach (var move in SolitaireGameStateViewModel.GetLegalMoves())
        {
            _shadowGameState.ExecuteMove(move);
            double eval = _evaluator.EvaluateState(_shadowGameState);
            _shadowGameState.UndoMove(move);
            LegalMoves.Add(new MoveViewModel(move, eval));
        }

        Evaluation = _evaluator.EvaluateState(_shadowGameState);
        OnPropertyChanged(nameof(SolitaireGameStateViewModel));
        OnPropertyChanged(nameof(Agent));
    }
}

public class AgentPlayingModel : AgentPlayingViewModel
{
    public static AgentPlayingModel Instance => new();

    AgentPlayingModel() : base()
    {
        Agent = new RandomSolitaireAgent();
    }
}