namespace SolvitaireCore;

public class SolitaireMoveGenerator 
{
    public IEnumerable<SolitaireMove> GenerateMoves(SolitaireGameState state)
    {
        var validMoves = new List<SolitaireMove>();

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
            foreach (var foundation in foundationPiles)
            {
                if (foundation.CanAddCard(topCard))
                {
                    validMoves.Add(new SingleCardMove(SolitaireGameState.WasteIndex, foundation.Index, topCard));
                    break; // Only one foundation move is possible
                }
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
            foreach (var foundation in foundationPiles)
            {
                if (topCard.Suit != foundation.Suit) continue; // Cannot move to a foundation of a different suit
                if (!foundation.CanAddCard(topCard)) continue;

                validMoves.Add(new SingleCardMove(tableau.Index, foundation.Index, topCard));
                break; // Only one foundation move is possible
            }
        }

        // Tableau → Tableau
        foreach (var tableau in tableauPiles)
        {
            if (tableau.IsEmpty) continue;

            var faceUpStartIndex = tableau.Cards.FindIndex(c => c.IsFaceUp);
            if (faceUpStartIndex == -1) continue;

            var faceUpCards = tableau.Cards.GetRange(faceUpStartIndex, tableau.Cards.Count - faceUpStartIndex);

            for (int i = 0; i < faceUpCards.Count; i++)
            {
                var cards = faceUpCards.TakeLast(i+1).ToList();
                foreach (var targetTableau in tableauPiles)
                {
                    if (targetTableau.Index == tableau.Index) 
                        continue;

                    var topCard = cards[0];
                    if (cards.Count == 1)
                    {
                        if (targetTableau.CanAddCard(topCard))
                            validMoves.Add(new SingleCardMove(tableau.Index, targetTableau.Index, topCard));
                        continue;
                    }
                    
                    if (targetTableau.Count > 0)
                    {
                        if (targetTableau.CurrentColor == topCard.Color)
                            continue; // Cannot move to a tableau of the same color
                        if (targetTableau.CurrentRank != topCard.Rank + 1)
                            continue; // Cannot move to a tableau of the same rank
                    }
                    else if (topCard.Rank != Rank.King)
                        continue;

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
                if (tableau.CurrentColor == topCard.Color) continue; // Cannot move to a tableau of the same color
                if (tableau.CurrentRank != topCard.Rank + 1) continue; // Cannot move to a tableau of the same rank

                validMoves.Add(new SingleCardMove(foundation.Index, tableau.Index, topCard));
            }
        }

        // Stock → Waste (Cycling)
        if (!stockPile.IsEmpty)
        {
            validMoves.Add(state.CycleMove);
        }

        // Waste → Stock (Recycle)
        if (stockPile.IsEmpty && !wastePile.IsEmpty && state.CycleCount <= state.MaximumCycles)
        {
            validMoves.Add(new MultiCardMove(wastePile.Index, stockPile.Index, wastePile.Cards));
        }

        return validMoves;
    }
}