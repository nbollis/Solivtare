namespace SolvitaireCore;

/// <summary>
/// Pile of cards for win condition, all cards of the same suit in ascending order.
/// </summary>
/// <param name="suit"></param>
/// <param name="initialCards"></param>
public class FoundationPile(Suit suit, int index = 0, IEnumerable<Card>? initialCards = null) 
    : Pile(index, initialCards)
{
    public Suit Suit { get; } = suit;

    public override bool CanAddCard(Card card)
    {
        if (Cards.Count == 0)
            return card.Rank == Rank.Ace && card.Suit == Suit;
        return card.Suit == Suit && card.Rank == TopCard!.Rank + 1;
    }

    public override string ToString() => $"Foundation[{Suit.ToSuitCharacter()}]";
}