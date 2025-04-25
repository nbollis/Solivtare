namespace SolvitaireCore;

public class MultiCardMove(Pile fromPile, Pile toPile, IEnumerable<Card> cards) : SolitaireMove(fromPile, toPile), IMove
{
    public List<Card> Cards { get; } = cards.ToList();
    private List<bool> _originalIsFaceUpStates;

    public override bool IsValid()
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

    public override void Execute()
    {
        if (IsValid())
        {
            _originalIsFaceUpStates = Cards.Select(card => card.IsFaceUp).ToList(); // Save the original states

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

    public override void Undo()
    {
        switch (ToPile)
        {
            case TableauPile toTableau when FromPile is TableauPile fromTableau:

                // tableau we pulled from still has cards
                if (FromPile.TopCard != null)
                {
                    // bottom of our stack does not fits on top of this one, so we should flip it back over
                    if (!FromPile.CanAddCard(Cards[0]))
                        FromPile.TopCard.IsFaceUp = false;

                }

                toTableau.RemoveCards(Cards);
                FromPile.Cards.AddRange(Cards);
                break;
            case WastePile:
                foreach (var card in Cards)
                {
                    ToPile.RemoveCard(card);
                    FromPile.Cards.Add(card);
                    card.IsFaceUp = false;
                }
                break;
            case StockPile:
                foreach (var card in Cards)
                {
                    ToPile.RemoveCard(card);
                    FromPile.AddCard(card);
                    card.IsFaceUp = true;
                }
                break;
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