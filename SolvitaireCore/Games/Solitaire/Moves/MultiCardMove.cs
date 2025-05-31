namespace SolvitaireCore;

public class MultiCardMove(int fromPileIndex, int toPileIndex, IEnumerable<Card> cards) 
    : SolitaireMove(fromPileIndex, toPileIndex), IMove<SolitaireGameState>
{
    public List<Card> Cards { get; } = cards.ToList();

    public override bool IsValid(SolitaireGameState gameState)
    {
        // To Waste -> Cycling 
        if (ToPileIndex == SolitaireGameState.WasteIndex)
        {
            return FromPileIndex == SolitaireGameState.StockIndex && gameState.StockPile.Count > 0;
        }

        if (ToPileIndex >= SolitaireGameState.FoundationStartIndex && ToPileIndex <= SolitaireGameState.FoundationEndIndex)
            return false; // Can't move multiple cards to foundation pile

        // To Stock (refresh)
        if (ToPileIndex == SolitaireGameState.StockIndex)
        {
            if (FromPileIndex != SolitaireGameState.WasteIndex)
                return false; // Can't move cards to stock pile unless it is from waste 
            if (gameState.StockPile.Count > 0)
                return false; // Can't move cards to stock pile unless it is empty
            if (gameState.WastePile.Count == 0 || gameState.WastePile.Count != Cards.Count)
                return false; // Can't move cards to stock pile unless it is from waste and waste has cards and we are moving all of them
            return true;
        }
            

        // checks that require access to the to pile
        var toPile = gameState.GetPileByIndex(ToPileIndex);
        if (toPile is TableauPile tableauPile)
        {
            return tableauPile.CanAddCards(Cards);
        }
        else
        {
            return false;
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
        return $"Move {Cards.Count} Cards ({Cards.First()}-{Cards.Last()}) from {fromPile} to {toPile}";
    }
}