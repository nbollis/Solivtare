using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireGuiFunctions;

namespace SolvitaireGUI;

public class AgentPlayingViewModel : BaseViewModel
{
    private GameStateViewModel _gameStateViewModel;
    private StandardDeck _deck;
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

    public GameStateViewModel GameStateViewModel
    {
        get => _gameStateViewModel;
        set
        {
            _gameStateViewModel = value;
            OnPropertyChanged(nameof(GameStateViewModel));
        }
    }

    public ICommand StartAgentCommand { get; set; }
    public ICommand ResetGameCommand { get; set; }
    public ICommand MakeMoveCommand { get; set; }
    public ICommand NewGameCommand { get; set; }
    public ICommand MakeSpecificMoveCommand { get; set; }

    public AgentPlayingViewModel()
    {
        _deck ??= new ObservableStandardDeck();
        _deck.Shuffle();

        var gameState = new SolitaireGameState();
        gameState.DealCards(_deck);

        GameStateViewModel = new GameStateViewModel(gameState);
        LegalMoves = new ObservableCollection<ISolitaireMove>(GameStateViewModel.GetLegalMoves());
        Agent = new RandomAgent();


        ResetGameCommand = new RelayCommand(ResetGame);
        MakeMoveCommand = new RelayCommand(MakeMove);
        NewGameCommand = new RelayCommand(NewGame);
        MakeSpecificMoveCommand = new DelegateCommand(MakeSpecificMove);
        StartAgentCommand = new RelayCommand(StartAgent);
    }

    private void ResetGame()
    {
        _deck.FlipAllCardsDown();
        var gameState = new SolitaireGameState();
        gameState.DealCards(_deck!);

        GameStateViewModel = new(gameState);
        Refresh();
    }

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

    private void MakeMove()
    {
        var move = Agent.GetNextMove(LegalMoves);
        GameStateViewModel.ApplyMove(move);
        Refresh();
    }

    private void MakeSpecificMove(object? moveObject)
    {
        if (moveObject is not ISolitaireMove move)
            return;
        GameStateViewModel.ApplyMove(move);
        Refresh();
    }

    private void NewGame()
    {
        _deck.Shuffle();
        ResetGame();
    }

    private async void StartAgent()
    {
        while (!GameStateViewModel.IsGameWon)
        {
            await Task.Run(() =>
            {
                var move = Agent.GetNextMove(LegalMoves);
                App.Current.Dispatcher.Invoke(() =>
                {
                    GameStateViewModel.ApplyMove(move);
                    Refresh();
                });
            });
            await Task.Delay(100); // Optional: Add a small delay for better UI responsiveness  
        }
    }

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