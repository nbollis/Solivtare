namespace SolvitaireCore;

/// <summary>
/// A move of a single card from one pile to another made by a agent.
/// </summary>
public class SingleCardMove(Pile fromPile, Pile toPile, Card card) : SolitaireMove(fromPile, toPile)
{
    public Card Card { get; } = card;
    private bool _originalIsFaceUp;
    private bool? _originalPreviousTableauCardIsFaceUp = null;
    public override bool IsValid()
    {
        if (FromPile.IsEmpty || !FromPile.TopCard.Equals(Card))
            return false;

        switch (ToPile)
        {
            case FoundationPile:
            case TableauPile:
                return ToPile.CanAddCard(Card);

            case WastePile:
                return true; // Stock and Waste piles accept any card  

            // This is false because only game resets can move cards from the stock pile, not a agent action
            case StockPile:
            default:
                return false;
        }
    }

    public override void Undo()
    {
        ToPile.RemoveCard(Card);
        switch (FromPile)
        {
            case TableauPile when ToPile is TableauPile:
                if (FromPile.TopCard != null && _originalPreviousTableauCardIsFaceUp != null)
                {
                    FromPile.TopCard.IsFaceUp = _originalPreviousTableauCardIsFaceUp.Value;
                }
                break;
        }
        Card.IsFaceUp = _originalIsFaceUp; // Restore the original face-up state
        FromPile.Cards.Add(Card);
    }

    public override void Execute()
    {
        if (IsValid())
        {
            _originalIsFaceUp = Card.IsFaceUp; // Save the original state
            FromPile.RemoveCard(Card);
            switch (FromPile)
            {
                case TableauPile:
                    if (FromPile.TopCard != null)
                    {
                        _originalPreviousTableauCardIsFaceUp = FromPile.TopCard.IsFaceUp;
                        FromPile.TopCard.IsFaceUp = true;
                    }
                    break;
            }
            ToPile.AddCard(Card);
        }
        else
        {
            throw new InvalidOperationException("Invalid move");
        }
    }

    public override string ToString() => $"Move {Card} from {FromPile} to {ToPile}";
}