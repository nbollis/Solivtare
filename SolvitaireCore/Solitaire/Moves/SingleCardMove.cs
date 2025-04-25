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
        var fromPile = gameState.GetPileByIndex(FromPileIndex);
        var toPile = gameState.GetPileByIndex(ToPileIndex);

        if (fromPile.IsEmpty || !fromPile.TopCard.Equals(Card))
            return false;

        switch (toPile)
        {
            case FoundationPile:
            case TableauPile:
                return toPile.CanAddCard(Card);

            case WastePile:
                return true; // Stock and Waste piles accept any card  

            // This is false because only game resets can move cards from the stock pile, not a agent action
            case StockPile:
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
                if (fromPile.TopCard != null && _originalPreviousTableauCardIsFaceUp != null)
                {
                    fromPile.TopCard.IsFaceUp = _originalPreviousTableauCardIsFaceUp.Value;
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

        if (IsValid(gameState))
        {
            _originalIsFaceUp = Card.IsFaceUp; // Save the original state
            fromPile.RemoveCard(Card);
            switch (fromPile)
            {
                case TableauPile:
                    if (fromPile.TopCard != null)
                    {
                        _originalPreviousTableauCardIsFaceUp = fromPile.TopCard.IsFaceUp;
                        fromPile.TopCard.IsFaceUp = true;
                    }
                    break;
            }
            toPile.AddCard(Card);
        }
        else
        {
            throw new InvalidOperationException("Invalid move");
        }
    }

    public override string ToString() => $"Move {Card} from {FromPileIndex} to {ToPileIndex}";
}