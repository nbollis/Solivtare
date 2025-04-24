namespace SolvitaireCore;

public class MultiCardMove(Pile fromPile, Pile toPile, IEnumerable<Card> cards) : SolitaireMove(fromPile, toPile), IMove
{
    public List<Card> Cards { get; } = cards.ToList();

    public override bool IsValid(GameState state)
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
                return FromPile is StockPile;

            case StockPile: // Stock pile is only valid for a full waste refresh
                return ToPile.Count == 0 && FromPile is WastePile waste && waste.Count == Cards.Count;

            default:
                return false;
        }
    }

    public override void Execute(GameState state)
    {
        if (IsValid(state))
        {
            switch (ToPile)
            {
                case TableauPile when FromPile is TableauPile fromTableau:
                    fromTableau.RemoveCards(Cards);
                    ToPile.AddCards(Cards);
                    if (FromPile.TopCard != null)
                        FromPile.TopCard.IsFaceUp = true;
                    break;

                case WastePile:
                    for (int i = Cards.Count - 1; i >= 0; i--)
                    {
                        var card = Cards[i];
                        FromPile.RemoveCard(card);
                        ToPile.AddCard(card);
                        card.IsFaceUp = true;
                    }
                    break;

                case StockPile:
                    for (int i = Cards.Count - 1; i >= 0; i--)
                    {
                        var card = Cards[i];
                        FromPile.RemoveCard(card);
                        ToPile.AddCard(card);
                        card.IsFaceUp = false;
                    }
                    break;
            }
        }
        else
        {
            throw new InvalidOperationException("Invalid move");
        }
    }

    public override string ToString()
    {
        if (ToPile is WastePile)
            return $"Cycle {Cards.Count} Cards";
        if (ToPile is StockPile)
            return $"Refresh Stock Pile";
        
        return $"Move {string.Join(',', Cards)} cards from {FromPile} to {ToPile}";
    }
}