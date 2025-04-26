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
        switch (toPile)
        {
            case TableauPile tableauPile:
                return tableauPile.CanAddCards(Cards);
            default:
                return false;
        }
    }

    public override void Execute(SolitaireGameState gameState)
    {
        var fromPile = gameState.GetPileByIndex(FromPileIndex);
        var toPile = gameState.GetPileByIndex(ToPileIndex);

        switch (toPile)
        {
            case TableauPile when fromPile is TableauPile fromTableau:
                fromTableau.RemoveCards(Cards);
                toPile.AddCards(Cards);
                if (fromPile.Count > 0)
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

    public override void Undo(SolitaireGameState gameState)
    {
        var fromPile = gameState.GetPileByIndex(FromPileIndex);
        var toPile = gameState.GetPileByIndex(ToPileIndex);
        switch (toPile)
        {
            case TableauPile toTableau when fromPile is TableauPile fromTableau:

                // tableau we pulled from still has cards
                if (fromPile.Count > 0)
                {
                    // bottom of our stack does not fits on top of this one, so we should flip it back over
                    if (!fromPile.CanAddCard(Cards[0]))
                        fromPile.TopCard!.IsFaceUp = false;

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