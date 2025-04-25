namespace SolvitaireCore;

public class MultiCardMove(int fromPileIndex, int toPileIndex, IEnumerable<Card> cards) 
    : SolitaireMove(fromPileIndex, toPileIndex), IMove<SolitaireGameState>
{
    public List<Card> Cards { get; } = cards.ToList();
    private List<bool> _originalIsFaceUpStates;

    public override bool IsValid(SolitaireGameState gameState)
    {
        var fromPile = gameState.GetPileByIndex(FromPileIndex);
        var toPile = gameState.GetPileByIndex(ToPileIndex);

        if (fromPile.IsEmpty || Cards.Any(card => !fromPile.Cards.Contains(card)))
            return false;

        switch (toPile)
        {
            case FoundationPile foundationPile:
                return false;

            case TableauPile tableauPile:
                return tableauPile.CanAddCards(Cards);

            case WastePile:
                return fromPile is StockPile;

            case StockPile: // Stock pile is only valid for a full waste refresh
                return toPile.Count == 0 && fromPile is WastePile waste && waste.Count == Cards.Count;

            default:
                return false;
        }
    }

    public override void Execute(SolitaireGameState gameState)
    {
        var fromPile = gameState.GetPileByIndex(FromPileIndex);
        var toPile = gameState.GetPileByIndex(ToPileIndex);

        if (IsValid(gameState))
        {
            _originalIsFaceUpStates = Cards.Select(card => card.IsFaceUp).ToList(); // Save the original states

            switch (toPile)
            {
                case TableauPile when fromPile is TableauPile fromTableau:
                    fromTableau.RemoveCards(Cards);
                    toPile.AddCards(Cards);
                    if (fromPile.TopCard != null)
                        fromPile.TopCard.IsFaceUp = true;
                    break;

                case WastePile:
                    for (int i = Cards.Count - 1; i >= 0; i--)
                    {
                        var card = Cards[i];
                        fromPile.RemoveCard(card);
                        toPile.AddCard(card);
                        card.IsFaceUp = true;
                    }
                    break;

                case StockPile:
                    for (int i = Cards.Count - 1; i >= 0; i--)
                    {
                        var card = Cards[i];
                        fromPile.RemoveCard(card);
                        toPile.AddCard(card);
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

    public override void Undo(SolitaireGameState gameState)
    {
        var fromPile = gameState.GetPileByIndex(FromPileIndex);
        var toPile = gameState.GetPileByIndex(ToPileIndex);
        switch (toPile)
        {
            case TableauPile toTableau when fromPile is TableauPile fromTableau:

                // tableau we pulled from still has cards
                if (fromPile.TopCard != null)
                {
                    // bottom of our stack does not fits on top of this one, so we should flip it back over
                    if (!fromPile.CanAddCard(Cards[0]))
                        fromPile.TopCard.IsFaceUp = false;

                }

                toTableau.RemoveCards(Cards);
                fromPile.Cards.AddRange(Cards);
                break;
            case WastePile:
                foreach (var card in Cards)
                {
                    toPile.RemoveCard(card);
                    fromPile.Cards.Add(card);
                    card.IsFaceUp = false;
                }
                break;
            case StockPile:
                foreach (var card in Cards)
                {
                    toPile.RemoveCard(card);
                    fromPile.AddCard(card);
                    card.IsFaceUp = true;
                }
                break;
        }

    }

    public override string ToString()
    {
        if (SolitaireGameState.GetPileStringByIndex(ToPileIndex) is "Waste")
            return $"Cycle {Cards.Count} Cards";
        if (SolitaireGameState.GetPileStringByIndex(ToPileIndex) is "Stock")
            return $"Refresh Stock Pile";

        var toPile = SolitaireGameState.GetPileStringByIndex(ToPileIndex);
        var fromPile = SolitaireGameState.GetPileStringByIndex(FromPileIndex);

        if (Cards.Count < 3)
            return $"Move {string.Join(',', Cards)} Cards from {fromPile} to {toPile}";
        return $"Move {Cards.Count} Cards ({Cards.First()}-{Cards.Last()}) from {fromPile} to {toPile} ()";
    }
}