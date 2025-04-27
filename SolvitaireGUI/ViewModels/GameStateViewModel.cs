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

    public void ApplyMove(SolitaireMove move)
    {
        BaseGameState.ExecuteMove(move);
        Sync(move);
    }

    public void UndoMove(SolitaireMove move)
    {
        BaseGameState.UndoMove(move);
        Sync(move);
    }

    public IEnumerable<SolitaireMove> GetLegalMoves()
    {
        var moves =  BaseGameState.GetLegalMoves();
        return moves;
    }

    private CancellationTokenSource? _syncDebounceToken;

    public async void Sync(SolitaireMove? move = null)
    {
        _syncDebounceToken?.Cancel();
        _syncDebounceToken = new CancellationTokenSource();
        var token = _syncDebounceToken.Token;

        try
        {
            await Task.Delay(50, token); // Debounce delay
            if (token.IsCancellationRequested) return;

            // Perform the sync logic
            if (move is not null)
            {
                if (move!.ToPileIndex >= SolitaireGameState.StockIndex ||
                    move.FromPileIndex >= SolitaireGameState.StockIndex)
                {
                    UpdateStockAndWaste();
                }

                if (move.ToPileIndex <= SolitaireGameState.TableauEndIndex || move.FromPileIndex <= SolitaireGameState.TableauEndIndex)
                    UpdateTableau();

                if (move.ToPileIndex >= SolitaireGameState.FoundationStartIndex && move.ToPileIndex <= SolitaireGameState.FoundationEndIndex
                    || move.FromPileIndex >= SolitaireGameState.FoundationStartIndex && move.FromPileIndex <= SolitaireGameState.FoundationEndIndex)
                    UpdateFoundation();
            }
            else
            {
                UpdateFoundation();
                UpdateStockAndWaste();
                UpdateTableau();
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
        }
    }

    public void UpdateStockAndWaste()
    {
        StockPile.UpdateFromPile(BaseGameState.StockPile);
        WastePile.UpdateFromPile(BaseGameState.WastePile);


        OnPropertyChanged(nameof(StockPile));
        OnPropertyChanged(nameof(WastePile));
    }

    public void UpdateTableau()
    {
        while (TableauPiles.Count < BaseGameState.TableauPiles.Count)
        {
            TableauPiles.Add(new BindablePile());
        }

        for (int i = 0; i < BaseGameState.TableauPiles.Count; i++)
        {
            TableauPiles[i].UpdateFromPile(BaseGameState.TableauPiles[i]);
        }
    }

    public void UpdateFoundation()
    {
        FoundationPiles.Clear();
        foreach (var pile in BaseGameState.FoundationPiles)
        {
            var bindable = new BindablePile();
            bindable.UpdateFromPile(pile);
            FoundationPiles.Add(bindable);
        }
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