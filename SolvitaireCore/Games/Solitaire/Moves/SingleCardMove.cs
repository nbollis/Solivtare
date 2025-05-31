namespace SolvitaireCore;

/// <summary>
/// A move of a single card from one pile to another made by a agent.
/// </summary>
public class SingleCardMove(int fromPileIndex, int toPileIndex, Card card) 
    : SolitaireMove(fromPileIndex, toPileIndex), IMove<SolitaireGameState>
{
    public Card Card { get; } = card;
    private bool _originalIsFaceUp;

    public override bool IsValid(SolitaireGameState gameState)
    {
        // Waste Pile accepts any card from stock pile
        if (ToPileIndex == SolitaireGameState.WasteIndex)
            return true;
        if (ToPileIndex == SolitaireGameState.StockIndex)
            return false; // Can't move cards to stock pile unless it is from waste and to is empty

        var toPile = gameState.GetPileByIndex(ToPileIndex);
        switch (toPile)
        {
            case FoundationPile:
            case TableauPile:
                return toPile.CanAddCard(Card);
            default:
                return false;
        }
    }

    public override string ToString()
    {
        return $"Move {Card} from {SolitaireGameState.GetPileStringByIndex(FromPileIndex)} to {SolitaireGameState.GetPileStringByIndex(ToPileIndex)}";
    }
}