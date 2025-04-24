namespace SolvitaireCore;

/// <summary>
/// A move of a single card from one pile to another made by a agent.
/// </summary>
public class SingleCardMove(Pile fromPile, Pile toPile, Card card) : SolitaireMove(fromPile, toPile), IMove
{
    public Card Card { get; } = card;

    public override bool IsValid(IGameState state)
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

    public override void Execute(IGameState state)
    {
        if (IsValid(state))
        {
            FromPile.RemoveCard(Card);
            switch (FromPile)
            {
                case TableauPile:
                    if (FromPile.TopCard != null) 
                        FromPile.TopCard.IsFaceUp = true;
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