﻿
namespace SolvitaireCore;

public class SolitaireGameState : BaseGameState<SolitaireMove>, IEquatable<SolitaireGameState>, ICardGameState
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
    public override bool IsGameLost => IsUnwinnable();
    public override bool IsGameWon => GameWon();

    // TODO: This is a bad way to get a parameterless constructor, fix. 
    public SolitaireGameState() : this(3){}
    public SolitaireGameState(int cardsPerCycle = 3)
    {
        TableauPiles = new List<TableauPile>();
        FoundationPiles = new List<FoundationPile>();

        int index = -1;
        for (int i = 0; i < 7; i++)
        {
            TableauPiles.Add(new TableauPile(++index));
        }
        for (int i = 0; i < 4; i++)
        {
            FoundationPiles.Add(new FoundationPile((Suit)i, ++index));
        }
        StockPile = new StockPile(++index);
        WastePile = new WastePile(++index);

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
            TableauPiles[i].Cards[^1].IsFaceUp = true;
            TableauPiles[i].Refresh();
        }

        while (cardIndex < 51)
        {
            StockPile.AddCard(deck[++cardIndex]);
        }
    }

    protected override void ResetInternal()
    {
        CycleCount = 0;
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
    public SolitaireMove CycleMove => new MultiCardMove(StockIndex, WasteIndex,
        StockPile.Cards.TakeLast(Math.Min(CardsPerCycle, StockPile.Count)));

    protected override List<SolitaireMove> GenerateLegalMoves()
    {
        var moves = MoveGenerator.GenerateMoves(this).ToList();
        moves.Add(new SkipGameMove()); // ← Add SkipMove
        return moves;
    }

    protected override void ExecuteMoveInternal(SolitaireMove move)
    {
        var undoInfo = new MoveUndoInfo();

        if (move.IsValid(this))
        {
            if (move.ToPileIndex == StockIndex)
                CycleCount++;

            if (move is SingleCardMove single)
            {
                var fromPile = GetPileByIndex(move.FromPileIndex);
                var toPile = GetPileByIndex(move.ToPileIndex);

                undoInfo.MovedCardWasFaceUp = single.Card.IsFaceUp;
                fromPile.RemoveCard(single.Card);
                toPile.AddCard(single.Card);
                if (fromPile is TableauPile && fromPile.Count > 0)
                {
                    undoInfo.PreviousTopCardWasFaceUp = fromPile.TopCard!.IsFaceUp;
                    fromPile.TopCard.IsFaceUp = true;
                }
            }
            else if (move is MultiCardMove multi)
            {
                if (multi.ToPileIndex == WasteIndex)
                {
                    for (int i = multi.Cards.Count - 1; i >= 0; i--)
                    {
                        var card = multi.Cards[i];
                        StockPile.RemoveCard(card);
                        WastePile.AddCard(card);
                        card.IsFaceUp = true;
                    }
                }
                else if (multi.ToPileIndex == StockIndex)
                {
                    for (int i = multi.Cards.Count - 1; i >= 0; i--)
                    {
                        var card = multi.Cards[i];
                        WastePile.RemoveCard(card);
                        card.IsFaceUp = false;
                        StockPile.AddCard(card);
                    }
                }
                else if (multi.ToPileIndex <= TableauEndIndex && multi.FromPileIndex <= TableauEndIndex)
                {
                    var fromPile = TableauPiles[multi.FromPileIndex];
                    var toPile = TableauPiles[multi.ToPileIndex];

                    fromPile.RemoveCards(multi.Cards);
                    toPile.AddCards(multi.Cards);

                    if (fromPile.Count > 0)
                    {
                        undoInfo.PreviousTopCardWasFaceUp = fromPile.TopCard!.IsFaceUp;
                        fromPile.TopCard.IsFaceUp = true;
                    }
                }
            }
            _undoStack.Push(undoInfo);
        }
        else
        {
            throw new InvalidOperationException("Invalid move");
        }
    }

    protected override void UndoMoveInternal(SolitaireMove move)
    {
        var undoInfo = _undoStack.Pop();

        if (move.ToPileIndex == StockIndex && move.FromPileIndex == WasteIndex)
            CycleCount--;
        if (move is SingleCardMove single)
        {
            if (single.ToPileIndex == WasteIndex)
            {
                WastePile.RemoveCard(single.Card);
                StockPile.AddCard(single.Card);
            }
            else if (single.ToPileIndex == StockIndex)
            {
                StockPile.RemoveCard(single.Card);
                WastePile.AddCard(single.Card);
            }

            var fromPile = GetPileByIndex(move.FromPileIndex);
            var toPile = GetPileByIndex(move.ToPileIndex);

            toPile.RemoveCard(single.Card);
            single.Card.IsFaceUp = undoInfo.MovedCardWasFaceUp ?? single.Card.IsFaceUp;

            // From tableau, pile currently still has cards, restore the previous face up state of the top card excluding the cards we are moving back. 
            if (single.FromPileIndex <= TableauEndIndex && fromPile.Count > 0)
            {
                fromPile.TopCard!.IsFaceUp = undoInfo.PreviousTopCardWasFaceUp ?? fromPile.TopCard.IsFaceUp;
            }
            fromPile.AddCard(single.Card);
        }
        else if (move is MultiCardMove multi)
        {
            if (multi.ToPileIndex == WasteIndex)
            {
                foreach (var card in multi.Cards)
                {
                    WastePile.RemoveCard(card);
                    StockPile.AddCard(card);
                    card.IsFaceUp = false;
                }
            }
            else if (multi.ToPileIndex == StockIndex)
            {
                foreach (var card in multi.Cards)
                {
                    StockPile.RemoveCard(card);
                    WastePile.AddCard(card);
                    card.IsFaceUp = true;
                }
            }
            else if (multi.ToPileIndex <= TableauEndIndex && multi.FromPileIndex <= TableauEndIndex)
            {
                var toPile = TableauPiles[multi.ToPileIndex];
                var fromPile = TableauPiles[multi.FromPileIndex];

                // From tableau, pile currently still has cards, restore the previous face up state of the top card excluding the cards we are moving back. 
                if (fromPile.Count > 0)
                {
                    fromPile.TopCard!.IsFaceUp = undoInfo.PreviousTopCardWasFaceUp ?? fromPile.TopCard.IsFaceUp;
                }

                fromPile.Cards.AddRange(multi.Cards);
                fromPile.Refresh();
                toPile.RemoveCards(multi.Cards);
            }
        }
    }

    // Track face up state of moved cards for undo functionality
    // currently assumes sequential operations (which is reasonable for solitaire)
    // To support non-sequential operations, we would need to track the state of each card individually or map the move undo info to the specific move or card moved.
    private readonly Stack<MoveUndoInfo> _undoStack = new();
    private class MoveUndoInfo
    {
        public bool? MovedCardWasFaceUp { get; set; }
        public bool? PreviousTopCardWasFaceUp { get; set; }
    }

    #endregion

    #region Pile Indexing

    public static int TableaStartIndex = 0;
    public static int TableauEndIndex = 6;
    public static int FoundationStartIndex = 7;
    public static int FoundationEndIndex = 10;
    public static int StockIndex = 11;
    public static int WasteIndex = 12;

    public FoundationPile this[Suit suit]
    {
        get => FoundationPiles[(int)suit];
        set => FoundationPiles[(int)suit] = value;
    }

    public Pile GetPileByIndex(int index)
    {
        if (index < TableauPiles.Count)
            return TableauPiles[index];
        index -= TableauPiles.Count;

        if (index < FoundationPiles.Count)
            return FoundationPiles[index];
        index -= FoundationPiles.Count;

        if (index == 0)
            return StockPile;
        index--;

        if (index == 0)
            return WastePile;

        throw new ArgumentOutOfRangeException(nameof(index), "Invalid pile index");
    }

    public static string GetPileStringByIndex(int index)
    {
        return index switch
        {
            < 7 => $"Tableau[{index + 1}]",
            < 11 => $"Foundation[{index - 6}]",
            11 => "Stock",
            12 => "Waste",
            _ => string.Empty
        };
    }

    #endregion

    #region Helpers

    public bool GameWon()
    {
        return FoundationPiles.All(pile => pile.Count == 13);
    }

    public bool IsUnwinnable()
    {
        // Rule 1: maximum cycles reached
        if (CycleCount >= MaximumCycles)
        {
            return true;
        }
        
        // Rule 2: No legal moves 
        if (!GetLegalMoves().Any())
        {
            return true;
        }

        // Rule 3: Check for blocked tableau
        bool allTableauBlocked = TableauPiles.All(pile =>
            pile.IsEmpty || !pile.Cards.Any(card => card.IsFaceUp && pile.CanRemoveCard(card)));
        if (allTableauBlocked)
        {
            return true;
        }

        // Rule 4: Check for foundation deadlock
        bool noFoundationMoves = !FoundationPiles.Any(foundation =>
            TableauPiles.Any(tableau => tableau.TopCard != null && foundation.CanAddCard(tableau.TopCard)) ||
            WastePile.TopCard != null && foundation.CanAddCard(WastePile.TopCard));
        if (noFoundationMoves)
        {
            return true;
        }

        return false; // If no rules indicate unwinnable, assume it's still winnable
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
            if (foundationPile.Suit != otherFoundationPile.Suit)
                return false;
            if (foundationPile.Count > 0 && !foundationPile.TopCard!.Equals(otherFoundationPile.TopCard))
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
                if (!tableauPile[i].Equals(otherTableauPile[i]))
                    return false;
            }
        }

        if (StockPile.Count != other.StockPile.Count)
            return false;
        if (WastePile.Count != other.WastePile.Count)
            return false;
        return true;
    }

    protected override int GenerateHashCode()
    {
        unchecked
        {
            var hash = new HashCode();

            // Hash FoundationPiles
            foreach (var foundationPile in FoundationPiles)
            {
                foreach (var card in foundationPile.Cards)
                {
                    hash.Add(card.GetHashCode());
                }
            }

            // Hash TableauPiles
            foreach (var tableauPile in TableauPiles)
            {
                foreach (var card in tableauPile.Cards)
                {
                    hash.Add(card.GetHashCode());
                }
            }

            // Hash StockPile
            foreach (var card in StockPile.Cards)
            {
                hash.Add(card.GetHashCode());
            }

            // Hash WastePile
            foreach (var card in WastePile.Cards)
            {
                hash.Add(card.GetHashCode());
            }

            // Include CycleCount and CardsPerCycle
            hash.Add(CycleCount);
            hash.Add(CardsPerCycle);

            return hash.ToHashCode();
        }
    }

    protected override IGameState<SolitaireMove> CloneInternal()
    {
        var clone = new SolitaireGameState(CardsPerCycle)
        {
            CycleCount = this.CycleCount
        };

        clone.StockPile = new StockPile(StockIndex, this.StockPile.Cards.Select(card => card.Clone()).ToList());
        clone.WastePile = new WastePile(WasteIndex,this.WastePile.Cards.Select(card => card.Clone()).ToList());
        clone.FoundationPiles = this.FoundationPiles.Select(pile => new FoundationPile(pile.Suit, pile.Index, pile.Cards.Select(card => card.Clone()).ToList())).ToList();
        clone.TableauPiles = this.TableauPiles.Select(pile => new TableauPile(pile.Index, pile.Cards.Select(card => card.Clone()).ToList())).ToList();

        return clone;
    }
}
