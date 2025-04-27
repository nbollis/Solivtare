namespace SolvitaireCore;

/// <summary>
/// Pile of cards that can be played on (columns in solitaire).
/// </summary>
public class TableauPile : Pile
{
    /// <summary>
    /// Pile of cards that can be played on (columns in solitaire).
    /// </summary>
    /// <param name="initialCards"></param>
    public TableauPile(int index = 0, IEnumerable<Card>? initialCards = null) : base(index, initialCards)
    {
        if (initialCards != null && Cards.Any())
        {
            CurrentRank = Cards[^1].Rank;
            CurrentColor = Cards[^1].Color;
        }
    }

    public Rank CurrentRank { get; private set; }
    public Color CurrentColor { get; private set; }

    public override bool CanAddCard(Card card)
    {
        if (Cards.Count == 0)
            return card.Rank == Rank.King;
        return card.Color != CurrentColor && card.Rank == CurrentRank - 1;
    }

    internal override void AddCard(Card card)
    {
        base.AddCard(card);
        CurrentRank = card.Rank;
        CurrentColor = card.Color;
    }

    public bool CanAddCards(List<Card> cards)
    {
        // if the pile is empty, the top card to be added must be a king
        if (Cards.Count == 0)
        {
            return cards[0].Rank == Rank.King;
        }

        // if the top card is not the same color as the bottom card, do not add
        if (TopCard.IsFaceUp)
        {
            if (cards[0].Color == CurrentColor)
                return false;

            if (cards[0].Rank == CurrentRank - 1)
                return true;
        }

        // if the card set is not alternating color and descending rank, do not add
        return IsValidCardSet(cards);
    }

    /// <summary>
    /// Adds multiple cards to the pile one at a time and checks if each card can be added prior to adding.
    /// </summary>
    /// <param name="cards">The collection of cards to add to the pile.</param>
    /// <exception cref="InvalidOperationException">Thrown if any card in the collection cannot be added to the pile.</exception>
    public override void AddCards(IEnumerable<Card> cards)
    {
        base.AddCards(cards);
        Refresh();
    }

    internal override void RemoveCard(Card card)
    {
        base.RemoveCard(card);
        Refresh();
    }

    public bool RemoveCards(List<Card> cards)
    {
        if (!cards[^1].Equals(Cards[^1]))
            throw new InvalidOperationException($"Cards to remove do not end with the tableau Top Card");

        int startIndex = Cards.IndexOf(cards[0]);
        if (startIndex == -1)
            throw new InvalidOperationException($"First card to remove not in Tableau");

        if (!IsValidCardSet(cards))
            throw new InvalidOperationException($"Cards to remove are not a valid set");

        // Replace RemoveRange with a loop to remove cards individually  
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            Cards.RemoveAt(startIndex + i);
        }

        Refresh();
        return true;
    }

    /// <summary>
    /// Determines if a valid set of cards can be played on this pile. 
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public static bool IsValidCardSet(List<Card> cards)
    {
        switch (cards.Count)
        {
            case 0:
                return false;
            case 1:
                return true;
        }

        for (int i = 1; i < cards.Count; i++)
        {
            if (cards[i].Color == cards[i - 1].Color || cards[i].Rank != cards[i - 1].Rank - 1)
                return false;
        }
        return true;
    }

    public void Refresh()
    {
        // Update the current rank and color based on the last card added
        if (TopCard == null)
        {
            CurrentColor = (Color)(-1);
            CurrentRank = (Rank)(-1);
        }
        else
        {
            CurrentColor = Cards[^1].Color;
            CurrentRank = Cards[^1].Rank;
        }
    }

    public override string ToString() => $"Tableau[{Index+1}]-{TopCard}";
}