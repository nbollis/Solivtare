using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireIO;

namespace SolvitaireGUI;

public class SolitaireGameStateViewModel : GameStateViewModel<SolitaireGameState, SolitaireMove>
{
    public SolitaireGameState BaseGameState { get; }

    public bool IsGameWon => BaseGameState.IsGameWon;

    public bool IsGameLost => BaseGameState.IsGameLost;


    public ObservableCollection<BindablePile> TableauPiles { get; } = new();
    public ObservableCollection<BindablePile> FoundationPiles { get; } = new();
    public BindablePile StockPile { get; } = new();
    public BindablePile WastePile { get; } = new();

    public ICommand CopyGameStateAsJsonCommand { get; }

    public SolitaireGameStateViewModel(SolitaireGameState gameState) : base(gameState)
    {
        BaseGameState = gameState;
        Sync();

        CopyGameStateAsJsonCommand = new RelayCommand(CopyGameStateAsJson);
    }

    public override void ApplyMove(SolitaireMove move)
    {
        BaseGameState.ExecuteMove(move);
        Sync(move);
    }

    public override void UndoMove(SolitaireMove move)
    {
        BaseGameState.UndoMove(move);
        Sync(move);
    }

    public IEnumerable<SolitaireMove> GetLegalMoves()
    {
        var moves = BaseGameState.GetLegalMoves();
        return moves;
    }

    private CancellationTokenSource? _syncDebounceToken;

    public override void UpdateBoard()
    {
        Sync();
    }

    public void Sync(SolitaireMove? move = null)
    {
        _syncDebounceToken?.Cancel();
        _syncDebounceToken = new CancellationTokenSource();
        var token = _syncDebounceToken.Token;

        try
        {
            Task.Delay(50, token).Wait(token); // Debounce delay
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

        OnPropertyChanged(nameof(IsGameWon));
        OnPropertyChanged(nameof(GameState));
        OnPropertyChanged(nameof(IsGameLost));
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
        TableauPiles.Clear();
        foreach (var pile in BaseGameState.TableauPiles)
        {
            var bindable = new BindablePile();
            bindable.UpdateFromPile(pile);
            TableauPiles.Add(bindable);
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

    private void CopyGameStateAsJson()
    {
        try
        {
            // Assuming GameState is a property in your ViewModel
            var gameStateJson = GameStateSerializer.Serialize(BaseGameState);

            Clipboard.SetText(gameStateJson);
            MessageBox.Show("GameState JSON copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to copy GameState JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public class SolitaireGameStateModel : SolitaireGameStateViewModel
{
    private static SolitaireGameState _solitaireGameState;
    public static SolitaireGameStateModel Instance => new();

    static SolitaireGameStateModel()
    {
        _solitaireGameState = new SolitaireGameState();
        var deck = new ObservableStandardDeck();
        deck.Shuffle();
        _solitaireGameState.DealCards(deck);
    }

    public SolitaireGameStateModel() : base(_solitaireGameState)
    {
        Sync();
    }
}