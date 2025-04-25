namespace SolvitaireCore;

public class SolitaireMoveGenerator 
{
    public IEnumerable<SolitaireMove> GenerateMoves(IGameState<SolitaireMove> state)
    {
        if (state is SolitaireGameState sol)
            return GenerateMoves(sol);
        return [];
    }

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
                if (foundation.CanAddCard(topCard))
                {
                    validMoves.Add(new SingleCardMove(tableau.Index, foundation.Index, topCard));
                    break; // Only one foundation move is possible
                }
            }
        }

        // Tableau → Tableau
        foreach (var tableau in tableauPiles)
        {
            if (tableau.IsEmpty) continue;

            var faceUpCards = tableau.Cards;
            var faceUpStartIndex = tableau.Cards.FindIndex(c => c.IsFaceUp);

            if (faceUpStartIndex == -1) continue;

            for (int i = faceUpStartIndex; i < faceUpCards.Count; i++)
            {
                var cardsToMove = faceUpCards.GetRange(i, faceUpCards.Count - i);

                foreach (var targetTableau in tableauPiles)
                {
                    if (targetTableau.Index == tableau.Index) continue;

                    if (cardsToMove.Count == 1 && targetTableau.CanAddCard(cardsToMove[0]))
                    {
                        validMoves.Add(new SingleCardMove(tableau.Index, targetTableau.Index, cardsToMove[0]));
                    }
                    else if (targetTableau.CanAddCards(cardsToMove))
                    {
                        validMoves.Add(new MultiCardMove(tableau.Index, targetTableau.Index, cardsToMove));
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
                if (tableau.CanAddCard(topCard))
                {
                    validMoves.Add(new SingleCardMove(foundation.Index, tableau.Index, topCard));
                }
            }
        }

        // Stock → Waste (Cycling)
        if (!stockPile.IsEmpty)
        {
            validMoves.Add(state.CycleMove);
        }

        // Waste → Stock (Recycle)
        if (stockPile.IsEmpty && !wastePile.IsEmpty)
        {
            validMoves.Add(new MultiCardMove(wastePile.Index, stockPile.Index, wastePile.Cards));
        }

        return validMoves;
    }
}