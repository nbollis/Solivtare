namespace SolvitaireCore;

/// <summary>
/// A move of a single card from one pile to another made by a agent.
/// </summary>
public class SingleCardMove(int fromPileIndex, int toPileIndex, Card card) 
    : SolitaireMove(fromPileIndex, toPileIndex), IMove<SolitaireGameState>
{
    public Card Card { get; } = card;
    private bool _originalIsFaceUp;
    private bool? _originalPreviousTableauCardIsFaceUp = null;

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

    public override void Undo(SolitaireGameState gameState)
    {
        var fromPile = gameState.GetPileByIndex(FromPileIndex);
        var toPile = gameState.GetPileByIndex(ToPileIndex);

        toPile.RemoveCard(Card);
        switch (fromPile)
        {
            case TableauPile when toPile is TableauPile:
                if (fromPile.Count > 0 && _originalPreviousTableauCardIsFaceUp != null)
                {
                    fromPile.TopCard!.IsFaceUp = _originalPreviousTableauCardIsFaceUp.Value;
                }
                break;
        }
        Card.IsFaceUp = _originalIsFaceUp; // Restore the original face-up state
        fromPile.Cards.Add(Card);
    }

    public override void Execute(SolitaireGameState gameState)
    {
        var fromPile = gameState.GetPileByIndex(FromPileIndex);
        var toPile = gameState.GetPileByIndex(ToPileIndex);

        _originalIsFaceUp = Card.IsFaceUp; // Save the original state
        fromPile.RemoveCard(Card);
        switch (fromPile)
        {
            case TableauPile:
                if (fromPile.Count > 0)
                {
                    _originalPreviousTableauCardIsFaceUp = fromPile.TopCard!.IsFaceUp;
                    fromPile.TopCard.IsFaceUp = true;
                }
                break;
        }
        toPile.AddCard(Card);
    }

    public override string ToString()
    {
        return $"Move {Card} from {SolitaireGameState.GetPileStringByIndex(FromPileIndex)} to {SolitaireGameState.GetPileStringByIndex(ToPileIndex)}";
    }
}