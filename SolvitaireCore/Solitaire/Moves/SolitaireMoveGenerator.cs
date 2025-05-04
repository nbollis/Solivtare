namespace SolvitaireCore;

public class SolitaireMoveGenerator 
{
    private readonly ListPool<SolitaireMove> _listPool = new();
    private readonly HashSetPool<Card> _hashSetPool = new();

    public IEnumerable<SolitaireMove> GenerateMoves(SolitaireGameState state)
    {
        // Get a pooled list for valid moves
        var validMoves = _listPool.Get();

        try
        {
            // Cache frequently accessed properties
            var wastePile = state.WastePile;
            var stockPile = state.StockPile;
            var tableauPiles = state.TableauPiles;
            var foundationPiles = state.FoundationPiles;

            // Waste → XX
            if (!wastePile.IsEmpty)
            {
                var topCard = wastePile.TopCard!;

                // Waste → Foundation
                var foundation = state[topCard.Suit];
                if (foundation.CanAddCard(topCard))
                {
                    validMoves.Add(new SingleCardMove(SolitaireGameState.WasteIndex, foundation.Index, topCard));
                }
                
                // Waste → Tableau
                foreach (var tableau in tableauPiles)
                {
                    if (tableau.CanAddCard(topCard))
                    {
                        validMoves.Add(new SingleCardMove(SolitaireGameState.WasteIndex, tableau.Index, topCard));
                    }
                }
            }

            // Tableau → Foundation
            foreach (var tableau in tableauPiles)
            {
                if (tableau.IsEmpty) continue;

                var topCard = tableau.TopCard!;
                var foundation = state[topCard.Suit];
                if (foundation.CanAddCard(topCard))
                {
                    validMoves.Add(new SingleCardMove(tableau.Index, foundation.Index, topCard));
                }
            }

            // Tableau → Tableau
            foreach (var tableau in tableauPiles)
            {
                if (tableau.IsEmpty) continue;

                var faceUpStartIndex = tableau.Cards.FindIndex(c => c.IsFaceUp);
                if (faceUpStartIndex == -1) continue;

                //var faceUpCards = tableau.Cards.GetRange(faceUpStartIndex, tableau.Cards.Count - faceUpStartIndex);
                for (int i = faceUpStartIndex; i < tableau.Count; i++)
                {
                    var cards = tableau.Cards.GetRange(i, tableau.Count - i);
                    foreach (var targetTableau in tableauPiles)
                    {
                        if (targetTableau.Index == tableau.Index)
                            continue;

                        // Use CanAddCards to validate the entire set of cards
                        if (targetTableau.CanAddCards(cards))
                        {
                            validMoves.Add(new MultiCardMove(tableau.Index, targetTableau.Index, cards));
                        }
                    }
                }
            }

            // Foundation → Tableau
            foreach (var foundation in foundationPiles)
            {
                if (foundation.IsEmpty) continue;

                var topCard = foundation.TopCard!;
                foreach (var tableau in tableauPiles)
                {
                    if (tableau.CurrentColor != topCard.Color && tableau.CurrentRank == topCard.Rank + 1)
                    {
                        validMoves.Add(new SingleCardMove(foundation.Index, tableau.Index, topCard));
                    }
                }
            }

            // Stock → Waste (Cycling)
            if (!stockPile.IsEmpty)
            {
                validMoves.Add(
                    new MultiCardMove(stockPile.Index, wastePile.Index,
                        stockPile.Cards.TakeLast(Math.Min(state.CardsPerCycle, stockPile.Count))));
            }

            // Waste → Stock (Recycle)
            if (stockPile.IsEmpty && !wastePile.IsEmpty && state.CycleCount <= state.MaximumCycles)
            {
                validMoves.Add(new MultiCardMove(wastePile.Index, stockPile.Index, wastePile.Cards));
            }

            // Return the valid moves as an enumerable
            return validMoves.ToList(); // Create a copy to return
        }
        finally
        {
            // Return the list to the pool
            _listPool.Return(validMoves);
        }
    }
}