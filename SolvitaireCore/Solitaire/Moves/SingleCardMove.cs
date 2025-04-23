namespace SolvitaireCore;

/// <summary>
/// A move of a single card from one pile to another made by a player.
/// </summary>
public class SingleCardMove(Pile fromPile, Pile toPile, Card card) : SolitaireMove(fromPile, toPile), IMove
{
    public Card Card { get; } = card;

    public override bool IsValid(GameState state)
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

            // This is false because only game resets can move cards from the stock pile, not a player action
            case StockPile:
            default:
                return false;
        }
    }

    public override void Execute(GameState state)
    {
        if (IsValid(state))
        {
            FromPile.RemoveCard(Card);
            ToPile.AddCard(Card);
        }
        else
        {
            throw new InvalidOperationException("Invalid move");
        }
    }

    public override string ToString() => $"Move {Card} from {FromPile} to {ToPile}";
}