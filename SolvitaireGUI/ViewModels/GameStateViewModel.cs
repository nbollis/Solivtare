using SolvitaireCore;
namespace SolvitaireGUI;

public class GameStateViewModel : BaseViewModel
{
    public GameStateViewModel(SolitaireGameState solitaireGameState)
    {
        SolitaireGameState = solitaireGameState;
    }

    private SolitaireGameState _solitaireGameState;
    public SolitaireGameState SolitaireGameState
    {
        get => _solitaireGameState;
        set
        {
            _solitaireGameState = value;
            OnPropertyChanged(nameof(SolitaireGameState));
            OnPropertyChanged(nameof(TableauPiles));
            OnPropertyChanged(nameof(FoundationPiles));
            OnPropertyChanged(nameof(StockPile));
            OnPropertyChanged(nameof(WastePile));
        }
    }

    public List<TableauPile> TableauPiles => SolitaireGameState.TableauPiles;
    public List<FoundationPile> FoundationPiles => SolitaireGameState.FoundationPiles;
    public StockPile StockPile => SolitaireGameState.StockPile;
    public WastePile WastePile => SolitaireGameState.WastePile;

    public void MakeMove(ISolitaireMove move)
    {
        SolitaireGameState.ExecuteMove(move);
    }

    public void Refresh() => OnPropertyChanged(string.Empty); // To manually refresh bindings
}

public class GameStateModel : GameStateViewModel
{
    private static SolitaireGameState _solitaireGameState;
    public static GameStateModel Instance => new();

    static GameStateModel()
    {
        _solitaireGameState = new SolitaireGameState();
        var deck = new StandardDeck();
        deck.Shuffle();
        _solitaireGameState.DealCards(deck);
    }

    public GameStateModel() : base(_solitaireGameState)
    {
        Refresh();
    }
}