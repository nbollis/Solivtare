using System.Collections;

namespace SolvitaireCore;

/// <summary>
/// An abstract stack of cards
/// </summary>
public abstract class Pile  : IEnumerable<Card>
{
    /// <summary>
    /// Represents a pile of cards in a solitaire game
    /// </summary>
    /// <remarks> Top card is the last card in the list. </remarks>
    public List<Card> Cards { get; }

    public readonly int Index;
    public int Count => Cards.Count;
    public bool IsEmpty => Cards.Count == 0;
    public Card? TopCard => Cards.LastOrDefault();
    public Card? BottomCard => Cards.FirstOrDefault();

    protected Pile(int index, IEnumerable<Card>? initialCards = null)
    {
        Index = index;

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

    internal virtual void AddCard(Card card) => Cards.Add(card);
    public bool TryAddCard(Card card)
    {
        if (CanAddCard(card))
        {
            AddCard(card);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Adds multiple cards to the pile one at a time and checks if each card can be added prior to adding.
    /// </summary>
    /// <param name="cards">The collection of cards to add to the pile.</param>
    /// <exception cref="InvalidOperationException">Thrown if any card in the collection cannot be added to the pile.</exception>
    public virtual void AddCards(IEnumerable<Card> cards)
    {
        foreach (var card in cards)
        {
            if (!CanAddCard(card))
                throw new InvalidOperationException($"Cannot add {card} to this pile.");
            AddCard(card);
        }
    }

    public virtual bool CanRemoveCard(Card card)
    {
        return !IsEmpty && card.Equals(TopCard);
    }

    public bool TryRemoveCard(Card card)
    {
        if (CanRemoveCard(card))
        {
            RemoveCard(card);
            return true;
        }
        return false;
    }

    internal virtual void RemoveCard(Card card)
    {
        Cards.Remove(card);
    }

    public IEnumerator<Card> GetEnumerator() => Cards.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}