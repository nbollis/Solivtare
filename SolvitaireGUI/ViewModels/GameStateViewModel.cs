using SolvitaireCore;
namespace SolvitaireGUI;

public class GameStateViewModel : BaseViewModel
{
    public GameStateViewModel(GameState gameState)
    {
        GameState = gameState;
    }

    private GameState _gameState;
    public GameState GameState
    {
        get => _gameState;
        set
        {
            _gameState = value;
            OnPropertyChanged(nameof(GameState));
            OnPropertyChanged(nameof(TableauPiles));
            OnPropertyChanged(nameof(FoundationPiles));
            OnPropertyChanged(nameof(StockPile));
            OnPropertyChanged(nameof(WastePile));
        }
    }

    public List<TableauPile> TableauPiles => GameState.TableauPiles;
    public List<FoundationPile> FoundationPiles => GameState.FoundationPiles;
    public StockPile StockPile => GameState.StockPile;
    public WastePile WastePile => GameState.WastePile;

    public void MakeMove(IMove move)
    {
        GameState.MoveCard(move);
        OnPropertyChanged(nameof(TableauPiles));
        OnPropertyChanged(nameof(FoundationPiles));
        OnPropertyChanged(nameof(StockPile));
        OnPropertyChanged(nameof(WastePile));
    }

    public void Refresh() => OnPropertyChanged(string.Empty); // To manually refresh bindings
}

public class GameStateModel : GameStateViewModel
{
    private static GameState _gameState;
    public static GameStateModel Instance => new();

    static GameStateModel()
    {
        _gameState = new GameState();
        var deck = new StandardDeck();
        deck.Shuffle();
        _gameState.DealCards(deck);
    }

    public GameStateModel() : base(_gameState)
    {
        Refresh();
    }
}