namespace SolvitaireCore;

/// <summary>
/// An abstract stack of cards
/// </summary>
public abstract class Pile 
{
    /// <summary>
    /// Represents a pile of cards in a solitaire game
    /// </summary>
    /// <remarks> Top card is the last card in the list. </remarks>
    public List<Card> Cards { get; } = [];
    public int Count => Cards.Count;
    public bool IsEmpty => Count == 0;

    public Card TopCard => IsEmpty ? throw new InvalidOperationException("Pile is empty.") : Cards[^1];
    public Card BottomCard => IsEmpty ? throw new InvalidOperationException("Pile is empty.") : Cards[0];

    protected Pile(IEnumerable<Card>? initialCards = null)
    {
        if (initialCards == null) return;
        foreach (var card in initialCards)
        {
            Cards.Add(card);
        }
    }

    public abstract bool CanAddCard(Card card);

    /// <summary>
    /// Adds a single card to the pile if the card can be added to the pile. 
    /// </summary>
    /// <param name="card">The card to add to the pile.</param>
    /// <exception cref="InvalidOperationException">Thrown if the card cannot be added to the pile.</exception>
    public void AddCard(Card card)
    {
        if (!CanAddCard(card))
            throw new InvalidOperationException($"Cannot add {card} to this pile.");
        Cards.Add(card);
    }

    /// <summary>
    /// Adds multiple cards to the pile one at a time and checks if each card can be added prior to adding.
    /// </summary>
    /// <param name="cards">The collection of cards to add to the pile.</param>
    /// <exception cref="InvalidOperationException">Thrown if any card in the collection cannot be added to the pile.</exception>
    public void AddCards(IEnumerable<Card> cards)
    {
        foreach (var card in cards)
        {
            if (!CanAddCard(card))
                throw new InvalidOperationException($"Cannot add {card} to this pile.");
            Cards.Add(card);
        }
    }

    public virtual bool CanRemoveCard(Card card)
    {
        return !IsEmpty && card.Equals(TopCard);
    }

    public void RemoveCard(Card card)
    {
        if (!CanRemoveCard(card))
            throw new InvalidOperationException($"Cannot remove {card} from this pile.");
        Cards.Remove(card);
    }

}

/// <summary>
/// Pile of cards for win condition, all cards of the same suit in ascending order.
/// </summary>
/// <param name="suit"></param>
/// <param name="initialCards"></param>
public class FoundationPile(Suit suit, IEnumerable<Card>? initialCards = null) : Pile(initialCards)
{
    public Suit Suit { get; } = suit;

    public override bool CanAddCard(Card card)
    {
        if (IsEmpty)
            return card.Rank == Rank.Ace && card.Suit == Suit;
        return card.Suit == Suit && card.Rank == TopCard.Rank + 1;
    }

    public override string ToString() => $"Foundation[{Suit}]";
}

/// <summary>
/// Pile of cards that can be played on (columns in solitaire).
/// </summary>
/// <param name="initialCards"></param>
public class TableauPile(int index = 0, IEnumerable<Card>? initialCards = null) : Pile(initialCards)
{
    public readonly int Index = index;
    public override bool CanAddCard(Card card)
    {
        if (IsEmpty)
            return card.Rank == Rank.King;
        return card.Color != TopCard.Color && card.Rank == TopCard.Rank - 1;
    }

    public bool CanAddCards(List<Card> cards)
    {
        // TODO: Indexes may be backwards, wait until implementation. 
        // if the pile is empty, the top card to be added must be a king
        if (IsEmpty && cards[0].Rank != Rank.King)
            return false;

        // if the card set is not alternating color and descending rank, do not add
        if (!IsValidCardSet(cards))
            return false;

        // if the bottom card to add is greater than the top card, do not add
        if (cards[0].Rank > TopCard.Rank)
            return false;

        // if the top card is not the same color as the bottom card, do not add
        if (cards[0].Color == TopCard.Color)
            return false;

        return true;
    }

    public bool RemoveCards(List<Card> cards)
    {
        if (!cards[^1].Equals(TopCard))
            throw new InvalidOperationException($"Cards to remove do not end with the tableau Top Card");
        
        int startIndex = Cards.IndexOf(cards[0]);
        if (startIndex == -1)
            throw new InvalidOperationException($"First card to remove not in Tableau");

        if (!IsValidCardSet(cards))
            throw new InvalidOperationException($"Cards to remove are not a valid set");

        Cards.RemoveRange(startIndex, cards.Count);
        return true;
    }

    /// <summary>
    /// Determines if a valid set of cards can be played on this pile. 
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public static bool IsValidCardSet(List<Card> cards)
    {
        if (!cards.Any()) return false;
        if (cards.Count == 1) return true;

        for (int i = 1; i < cards.Count; i++)
        {
            if (cards[i].Color == cards[i - 1].Color || cards[i].Rank != cards[i - 1].Rank - 1)
                return false;
        }
        return true;
    }

    public override string ToString() => $"Tableau[{Index}]";
}

/// <summary>
/// Deck which playable cards are drawn from.
/// </summary>
/// <param name="initialCards"></param>
public class StockPile(IEnumerable<Card>? initialCards = null) : Pile(initialCards)
{
    public override bool CanAddCard(Card card) => true; // Stock pile can accept any card
    public override string ToString() => "Stock"; 
}


/// <summary>
/// Top card only can be used for play
/// </summary>
/// <param name="initialCards"></param>
public class WastePile(IEnumerable<Card>? initialCards = null) : Pile(initialCards)
{
    public override bool CanAddCard(Card card) => true; // Waste pile can accept any card
    public override string ToString() => "Waste";
}
