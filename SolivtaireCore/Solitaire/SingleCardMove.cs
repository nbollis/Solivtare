namespace SolivtaireCore;

/// <summary>
/// A move of a single card from one pile to another made by a player.
/// </summary>
public class SingleCardMove(Pile fromPile, Pile toPile, Card card) : IMove
{
    public Pile FromPile { get; } = fromPile;
    public Pile ToPile { get; } = toPile;
    public Card Card { get; } = card;

    public bool IsValid(GameState state)
    {
        if (FromPile.IsEmpty || !FromPile.TopCard.Equals(Card))
            return false;

        switch (ToPile)
        {
            case FoundationPile foundationPile:
            case TableauPile tableauPile:
                return FromPile.CanAddCard(Card);

            case WastePile:
                return true; // Stock and Waste piles accept any card  

            // This is false because only game resets can move cards from the stock pile, not a player action
            case StockPile:
            default:
                return false;
        }
    }

    public void Execute(GameState state)
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
}

public class MultiCardMove(Pile fromPile, Pile toPile, List<Card> cards) : IMove
{
    public Pile FromPile { get; } = fromPile;
    public Pile ToPile { get; } = toPile;
    public List<Card> Cards { get; } = cards;

    public bool IsValid(GameState state)
    {
        if (FromPile.IsEmpty || !Cards.All(card => FromPile.Cards.Contains(card)))
            return false;

        switch (ToPile)
        {
            case FoundationPile foundationPile:
                return false;

            case TableauPile tableauPile:
                return tableauPile.CanAddCards(Cards);

            case WastePile:
                return true; // Waste piles accept any card  

            // This is false because only game resets can move cards from the stock pile, not a player action
            case StockPile:
            default:
                return false;
        }
    }

    public void Execute(GameState state)
    {
        if (IsValid(state))
        {
            switch (ToPile)
            {
                case TableauPile tableauPile:

                    // TODO: Implement logic for multiple card moves to tableau piles
                    break;
                case WastePile:
                {
                    foreach (var card in Cards)
                    {
                        FromPile.RemoveCard(card);
                        ToPile.AddCard(card);
                        card.IsFaceUp = true;
                    }

                    break;
                }
                case StockPile:
                {
                    foreach (var card in Cards)
                    {
                        FromPile.RemoveCard(card);
                        ToPile.AddCard(card);
                        card.IsFaceUp = false;
                    }
                    break;
                }
            }
        }
        else
        {
            throw new InvalidOperationException("Invalid move");
        }
    }
}