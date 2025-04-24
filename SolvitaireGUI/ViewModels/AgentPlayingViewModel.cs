using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireGuiFunctions;

namespace SolvitaireGUI;

public class AgentPlayingViewModel : BaseViewModel
{
    private Stack<ISolitaireMove> _previousMoves;
    private GameStateViewModel _gameStateViewModel;
    private StandardDeck _deck;
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
        _previousMoves = new();
        _deck ??= new ObservableStandardDeck(23);
        _deck.Shuffle();

        var gameState = new SolitaireGameState();
        gameState.DealCards(_deck);

        GameStateViewModel = new GameStateViewModel(gameState);
        LegalMoves = new ObservableCollection<ISolitaireMove>(GameStateViewModel.GetLegalMoves());
        Agent = new RandomAgent();
        AllAgents = new()
        {
            Agent
        };

        ResetGameCommand = new RelayCommand(ResetGame);
        MakeMoveCommand = new RelayCommand(AgentMakeMove);
        UndoMoveCommand = new RelayCommand(UndoMove);
        NewGameCommand = new RelayCommand(NewGame);
        MakeSpecificMoveCommand = new DelegateCommand(MakeSpecificMove);
        StartAgentCommand = new RelayCommand(StartAgent);
        StopAgentCommand = new RelayCommand(StopAgent);
    }


    #region Agent Playing

    private CancellationTokenSource? _agentCancellationTokenSource;
    private ISolitaireAgent _agent;

    public ISolitaireAgent Agent
    {
        get => _agent;
        set
        {
            _agent = value;
            OnPropertyChanged(nameof(Agent));
        }
    }

    public ObservableCollection<ISolitaireAgent> AllAgents { get; set; }

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
                await Task.Run(() =>
                {
                    var move = Agent.GetNextMove(LegalMoves);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        GameStateViewModel.ApplyMove(move);
                        _previousMoves.Push(move);
                        Refresh();
                    });
                }, token);

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
        var move = Agent.GetNextMove(LegalMoves);
        GameStateViewModel.ApplyMove(move);
        _previousMoves.Push(move);
        Refresh();
    }

    #endregion

    #region Human Interaction

    private IMove _selectedMove;
    public IMove SelectedMove
    {
        get => _selectedMove;
        set
        {
            _selectedMove = value;
            OnPropertyChanged(nameof(SelectedMove));
        }
    }

    private ObservableCollection<ISolitaireMove> _legalMoves;
    public ObservableCollection<ISolitaireMove> LegalMoves
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
        if (moveObject is not ISolitaireMove move)
            return;
        GameStateViewModel.ApplyMove(move);
        _previousMoves.Push(move);
        Refresh();
    }

    private void UndoMove()
    {
        if (!_previousMoves.TryPop(out var move)) 
            return;
        GameStateViewModel.UndoMove(move);
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

        GameStateViewModel = new(gameState);
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
        LegalMoves = new ObservableCollection<ISolitaireMove>(GameStateViewModel.GetLegalMoves());
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