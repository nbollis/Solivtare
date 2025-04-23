namespace SolvitaireCore;

/// <summary>
/// Deck which playable cards are drawn from.
/// </summary>
/// <param name="initialCards"></param>
public class StockPile(IEnumerable<Card>? initialCards = null) : Pile(initialCards)
{
    public override bool CanAddCard(Card card) => true; // Stock pile can accept any card
    public override string ToString() => "Stock"; 
}