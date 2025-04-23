namespace SolvitaireCore;

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