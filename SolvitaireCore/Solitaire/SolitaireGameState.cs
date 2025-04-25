using System.Collections.ObjectModel;
namespace SolvitaireCore;

public class SolitaireGameState : IGameState<ISolitaireMove>
{
    // This is the number of cards to move from stock to waste when cycling
    public readonly int CardsPerCycle;
    public readonly int MaximumCycles = int.MaxValue;
    public int CycleCount = 0;

    public List<TableauPile> TableauPiles { get; set; }

    public List<FoundationPile> FoundationPiles { get; set; }

    public StockPile StockPile { get; set; }

    public WastePile WastePile { get; set; }

    // TODO: Find more lose conditions: Those that involve an infinite loop
    public bool IsGameLost => !GetLegalMoves().Any() || CycleCount > MaximumCycles;
    public bool IsGameWon => FoundationPiles.All(pile => pile.Count == 13);


    public SolitaireGameState(int cardsPerCycle = 3)
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

    #region Set Up SolitaireGameState
    
    public void DealCards(StandardDeck deck)
    {
        int cardIndex = -1;
        deck.FlipAllCardsDown();
        Card[][] tableauCards = new Card[TableauPiles.Count][];
        for (int i = 0; i < TableauPiles.Count; i++)
        {
            tableauCards[i] = new Card[i + 1];
            for (int j = 0; j < tableauCards[i].Length; j++)
            {
                tableauCards[i][j] = deck[++cardIndex];
            }
        }
        for (int i = 0; i < TableauPiles.Count; i++)
        {
            // AddRange here instead of add range so we do not trigger the validity check on deal
            TableauPiles[i].Cards.AddRange(tableauCards[i]);
            TableauPiles[i].Cards[^1].IsFaceUp = true; // flip the last card face up
        }

        while (cardIndex < 51)
        {
            StockPile.AddCard(deck[++cardIndex]);
        }
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
    }

    #endregion

    #region Move Making

    private static readonly SolitaireMoveGenerator MoveGenerator = new();
    /// <summary>
    /// Moves cards from Stock to Waste pile. Moves CardsPerCycle or all remaining cards, whichever is less.
    /// </summary>
    public ISolitaireMove CycleMove => new MultiCardMove(StockPile, WastePile,
        StockPile.Cards.TakeLast(Math.Min(CardsPerCycle, StockPile.Count)).ToList());
    
    public IEnumerable<ISolitaireMove> GetLegalMoves()
    {
        return MoveGenerator.GenerateMoves(this);
    }

    public void ExecuteMove(ISolitaireMove move)
    {
        if (move.IsValid())
        {
            move.Execute();
        }
        else
        {
            throw new InvalidOperationException("Invalid move");
        }
    }

    public void UndoMove(ISolitaireMove move)
    {
        move.Undo();
    }

    #endregion

    public bool Equals(SolitaireGameState? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        for (var index = 0; index < FoundationPiles.Count; index++)
        {
            var foundationPile = FoundationPiles[index];
            var otherFoundationPile = other.FoundationPiles[index];
            if (foundationPile.Count != otherFoundationPile.Count)
                return false;
            if (foundationPile.TopCard != otherFoundationPile.TopCard)
                return false;
        }

        for (var index = 0; index < TableauPiles.Count; index++)
        {
            var tableauPile = TableauPiles[index];
            var otherTableauPile = other.TableauPiles[index];
            if (tableauPile.Count != otherTableauPile.Count)
                return false;
            for (int i = 0; i < tableauPile.Count; i++)
            {
                if (tableauPile[i] != otherTableauPile[i])
                    return false;
            }
        }

        if (StockPile.Count != other.StockPile.Count)
            return false;
        if (WastePile.Count != other.WastePile.Count)
            return false;
        return true;
    }

    public SolitaireGameState Clone()
    {
        var clone = new SolitaireGameState(CardsPerCycle)
        {
            CycleCount = this.CycleCount
        };

        clone.StockPile = new StockPile(this.StockPile.Cards.Select(card => card.Clone()).ToList());
        clone.WastePile = new WastePile(this.WastePile.Cards.Select(card => card.Clone()).ToList());
        clone.FoundationPiles = this.FoundationPiles.Select(pile => new FoundationPile(pile.Suit, pile.Cards.Select(card => card.Clone()).ToList())).ToList();
        clone.TableauPiles = this.TableauPiles.Select(pile => new TableauPile(pile.Index, pile.Cards.Select(card => card.Clone()).ToList())).ToList();

        return clone;
    }
}
