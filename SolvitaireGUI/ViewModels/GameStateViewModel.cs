using SolvitaireCore;
using System.Collections.ObjectModel;
namespace SolvitaireGUI;

public class GameStateViewModel : BaseViewModel
{
    public SolitaireGameState BaseGameState { get; }

    public bool IsGameWon => BaseGameState.IsGameWon;
    public bool IsGameLost => BaseGameState.IsGameLost;


    public ObservableCollection<BindablePile> TableauPiles { get; } = new();
    public ObservableCollection<BindablePile> FoundationPiles { get; } = new();
    public BindablePile StockPile { get; } = new();
    public BindablePile WastePile { get; } = new();


    public GameStateViewModel(SolitaireGameState gameState)
    {
        BaseGameState = gameState;
        Sync();
    }

    public void ApplyMove(ISolitaireMove move)
    {
        BaseGameState.ExecuteMove(move);
        Sync();
    }

    public IEnumerable<ISolitaireMove> GetLegalMoves()
    {
        return BaseGameState.GetLegalMoves();
    }

    public void Sync()
    {
        TableauPiles.Clear();
        foreach (var pile in BaseGameState.TableauPiles)
        {
            var bindable = new BindablePile();
            bindable.UpdateFromPile(pile);
            TableauPiles.Add(bindable);
        }

        FoundationPiles.Clear();
        foreach (var pile in BaseGameState.FoundationPiles)
        {
            var bindable = new BindablePile();
            bindable.UpdateFromPile(pile);
            FoundationPiles.Add(bindable);
        }

        StockPile.UpdateFromPile(BaseGameState.StockPile);
        WastePile.UpdateFromPile(BaseGameState.WastePile);

        OnPropertyChanged(nameof(TableauPiles));
        OnPropertyChanged(nameof(FoundationPiles));
        OnPropertyChanged(nameof(StockPile));
        OnPropertyChanged(nameof(WastePile));
    }
}

public class GameStateModel : GameStateViewModel
{
    private static SolitaireGameState _solitaireGameState;
    public static GameStateModel Instance => new();

    static GameStateModel()
    {
        _solitaireGameState = new SolitaireGameState();
        var deck = new ObservableStandardDeck();
        deck.Shuffle();
        _solitaireGameState.DealCards(deck);
    }

    public GameStateModel() : base(_solitaireGameState)
    {
        Sync();
    }
}