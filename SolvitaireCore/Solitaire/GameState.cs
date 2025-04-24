using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SolvitaireCore;

public class GameState : INotifyPropertyChanged
{
    // This is the number of cards to move from stock to waste when cycling
    public readonly int CardsPerCycle;

    #region Piles

    private List<TableauPile> _tableauPiles;
    public List<TableauPile> TableauPiles
    {
        get => _tableauPiles;
        set
        {
            _tableauPiles = value;
            OnPropertyChanged();
        }
    }

    private List<FoundationPile> _foundationPiles;
    public List<FoundationPile> FoundationPiles
    {
        get => _foundationPiles;
        set
        {
            _foundationPiles = value;
            OnPropertyChanged();
        }
    }

    private StockPile _stockPile;
    public StockPile StockPile
    {
        get => _stockPile;
        set
        {
            _stockPile = value;
            OnPropertyChanged();
        }
    }

    private WastePile _wastePile;
    public WastePile WastePile
    {
        get => _wastePile;
        set
        {
            _wastePile = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public bool IsGameWon => FoundationPiles.All(pile => pile.Count == 13);

    public GameState(int cardsPerCycle = 3)
    {
        StockPile = new StockPile();
        WastePile = new WastePile();
        TableauPiles = new List<TableauPile>();
        FoundationPiles = new List<FoundationPile>();

        for (int i = 0; i < 4; i++)
        {
            FoundationPiles.Add(new FoundationPile((Suit)i));
        }
        for (int i = 0; i < 7; i++)
        {
            TableauPiles.Add(new TableauPile(i));
        }

        CardsPerCycle = cardsPerCycle;
    }

    #region Set Up GameState
    
    public void DealCards(StandardDeck deck)
    {
        Card[][] tableauCards = new Card[TableauPiles.Count][];
        for (int i = 0; i < TableauPiles.Count; i++)
        {
            tableauCards[i] = new Card[i + 1];
            for (int j = 0; j < tableauCards[i].Length; j++)
            {
                tableauCards[i][j] = deck.DrawCard();
            }
        }
        for (int i = 0; i < TableauPiles.Count; i++)
        {
            // new collection here instead of add range so we do not trigger the validity check on deal
            TableauPiles[i].Cards = new ObservableCollection<Card>(tableauCards[i]);
            TableauPiles[i].Cards[^1].IsFaceUp = true; // flip the last card face up
        }

        while (deck.Cards.Count > 0)
        {
            StockPile.AddCard(deck.DrawCard());
        }

        RefreshUi();
    }

    public void Reset()
    {
        foreach (var pile in FoundationPiles)
        {
            pile.Cards.Clear();
        }
        foreach (var pile in TableauPiles)
        {
            pile.Cards.Clear();
        }
        StockPile.Cards.Clear();
        WastePile.Cards.Clear();

        RefreshUi();
    }

    #endregion

    #region Move Making

    private static readonly SolitaireMoveGenerator MoveGenerator = new();
    /// <summary>
    /// Moves cards from Stock to Waste pile. Moves CardsPerCycle or all remaining cards, whichever is less.
    /// </summary>
    public IMove CycleMove => new MultiCardMove(StockPile, WastePile,
        StockPile.Cards.TakeLast(Math.Min(CardsPerCycle, StockPile.Count)).ToList());
    
    public IEnumerable<IMove> GetLegalMoves()
    {
        return MoveGenerator.GenerateMoves(this);
    }

    public void MoveCard(IMove move)
    {
        if (move.IsValid(this))
        {
            move.Execute(this);
        }
        else
        {
            throw new InvalidOperationException("Invalid move");
        }
        RefreshUi();
    }

    #endregion


    

    

    #region UI Interaction

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null!) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void RefreshUi()
    {
        OnPropertyChanged(nameof(StockPile));
        OnPropertyChanged(nameof(WastePile));
        OnPropertyChanged(nameof(TableauPiles));
        OnPropertyChanged(nameof(FoundationPiles));
    }

    #endregion
}
