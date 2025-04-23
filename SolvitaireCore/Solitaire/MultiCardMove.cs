namespace SolvitaireCore;

public class MultiCardMove(Pile fromPile, Pile toPile, List<Card> cards) : SolitaireMove(fromPile, toPile), IMove
{
    public List<Card> Cards { get; } = cards;

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
                case TableauPile when FromPile is TableauPile fromTableau :
                    fromTableau.RemoveCards(cards);
                    ToPile.AddCards(cards);
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

    public override string ToString() => $"Move {string.Join(',', Cards)} cards from {FromPile} to {ToPile}";
}