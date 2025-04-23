namespace SolivtaireCore;

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

    //public abstract bool CanTakeCard();
    //public abstract bool CanTakeCards(IEnumerable<Card> cards);

    public abstract bool CanAddCard(Card card);

    public void AddCard(Card card)
    {
        if (!CanAddCard(card))
            throw new InvalidOperationException($"Cannot add {card} to this pile.");
        Cards.Add(card);
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
}

/// <summary>
/// Pile of cards that can be played on (columns in solitaire).
/// </summary>
/// <param name="initialCards"></param>
public class TableauPile(IEnumerable<Card>? initialCards = null) : Pile(initialCards)
{
    public override bool CanAddCard(Card card)
    {
        if (IsEmpty)
            return card.Rank == Rank.King;
        return card.Color != TopCard.Color && card.Rank == TopCard.Rank - 1;
    }
}

/// <summary>
/// Deck which playable cards are drawn from.
/// </summary>
/// <param name="initialCards"></param>
public class StockPile(IEnumerable<Card>? initialCards = null) : Pile(initialCards)
{
    public override bool CanAddCard(Card card) => true; // Stock pile can accept any card
}


/// <summary>
/// Top card only can be used for play
/// </summary>
/// <param name="initialCards"></param>
public class WastePile(IEnumerable<Card>? initialCards = null) : Pile(initialCards)
{
    public override bool CanAddCard(Card card) => true; // Waste pile can accept any card
}
