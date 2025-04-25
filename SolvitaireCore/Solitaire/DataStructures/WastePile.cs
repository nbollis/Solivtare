namespace SolvitaireCore;

/// <summary>
/// Top card only can be used for play
/// </summary>
/// <param name="initialCards"></param>
public class WastePile(int? index = null,IEnumerable<Card>? initialCards = null) 
    : Pile(index ?? SolitaireGameState.WasteIndex,initialCards)
{
    public override bool CanAddCard(Card card) => true; // Waste pile can accept any card
    public override string ToString() => "Waste";
}