namespace SolvitaireCore;

/// <summary>
/// Deck which playable cards are drawn from.
/// </summary>
/// <param name="initialCards"></param>
public class StockPile(int? index = null, IEnumerable<Card>? initialCards = null) 
    : Pile(index ?? SolitaireGameState.StockIndex, initialCards)
{
    public override bool CanAddCard(Card card) => true; // Stock pile can accept any card
    public override string ToString() => "Stock";
}