using System.Collections.ObjectModel;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireGuiFunctions;

namespace SolvitaireGUI;

public class AgentPlayingViewModel : BaseViewModel
{
    private StandardDeck _deck;
    private IAgent _agent;

    public IAgent Agent
    {
        get => _agent;
        set
        {
            _agent = value;
            OnPropertyChanged(nameof(Agent));
        }
    }

    public GameStateViewModel GameStateViewModel { get; set; }


    public ICommand StartAgentCommand { get; set; }
    public ICommand ResetGameCommand { get; set; }
    public ICommand MakeMoveCommand { get; set; }
    public ICommand NewGameCommand { get; set; }
    public ICommand MakeSpecificMoveCommand { get; set; }

    public AgentPlayingViewModel()
    {
        _deck ??= new StandardDeck();
        _deck.Shuffle();

        var deck = _deck.Clone() as StandardDeck;
        var gameState = new GameState();
        gameState.DealCards(deck);
        GameStateViewModel = new GameStateViewModel(gameState);
        LegalMoves = new ObservableCollection<IMove>(GameStateViewModel.GameState.GetLegalMoves());
        Agent = new RandomAgent();


        ResetGameCommand = new RelayCommand(ResetGame);
        MakeMoveCommand = new RelayCommand(MakeMove);
        NewGameCommand = new RelayCommand(NewGame);
        MakeSpecificMoveCommand = new DelegateCommand(MakeSpecificMove);
        StartAgentCommand = new RelayCommand(StartAgent);
    }

    private void ResetGame()
    {
        var deck = _deck.Clone() as StandardDeck ?? throw new InvalidCastException();
        foreach (var card in deck)
            card.IsFaceUp = false;

        var gameState = new GameState();
        gameState.DealCards(deck!);
        GameStateViewModel.GameState = gameState;
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

    private ObservableCollection<IMove> _legalMoves;
    public ObservableCollection<IMove> LegalMoves
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
        GameStateViewModel.MakeMove(move);
        Refresh();
    }

    private void MakeSpecificMove(object? moveObject)
    {
        if (moveObject is not IMove move)
            return;
        GameStateViewModel.MakeMove(move);
        Refresh();
    }

    private void NewGame()
    {
        _deck.Shuffle();
        ResetGame();
    }

    private async void StartAgent()
    {
        while (!GameStateViewModel.GameState.IsGameWon)
        {
            await Task.Run(() =>
            {
                var move = Agent.GetNextMove(LegalMoves);
                App.Current.Dispatcher.Invoke(() =>
                {
                    GameStateViewModel.MakeMove(move);
                    Refresh();
                });
            });
            await Task.Delay(100); // Optional: Add a small delay for better UI responsiveness  
        }
    }

    public void Refresh()
    {
        LegalMoves = new ObservableCollection<IMove>(GameStateViewModel.GameState.GetLegalMoves());
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