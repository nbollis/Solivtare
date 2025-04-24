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
    public List<Card> Cards { get;  }

    public int Count => Cards.Count;
    public bool IsEmpty => Count == 0;
    public Card? TopCard => Cards.LastOrDefault();
    public Card? BottomCard => Cards.FirstOrDefault();

    protected Pile(IEnumerable<Card>? initialCards = null)
    {
        Cards = [];
        if (initialCards == null) return;
        foreach (var card in initialCards)
        {
            Cards.Add(card);
        }
    }

    public Card this[int index]
    {
        get => Cards[index];
        set => Cards[index] = value;
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