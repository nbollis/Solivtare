namespace SolvitaireCore;

public class SolitaireMoveGenerator : IMoveGenerator
{
    public IEnumerable<IMove> GenerateMoves(GameState state)
    {
        var validMoves = new List<IMove>();

        // Waste → XX
        if (!state.WastePile.IsEmpty)
        {
            var topCard = state.WastePile.TopCard;

            // Waste → Foundation
            foreach (var foundation in state.FoundationPiles)
            {
                if (!foundation.CanAddCard(state.WastePile.TopCard))
                    continue;

                validMoves.Add(new SingleCardMove(state.WastePile, foundation, topCard));
                break;
            }

            // Waste → Tableau
            foreach (var tableau in state.TableauPiles)
            {
                if (tableau.CanAddCard(topCard))
                {
                    validMoves.Add(new SingleCardMove(state.WastePile, tableau, topCard));
                }
            }
        }

        // Tableau → Foundation
        foreach (var tableau in state.TableauPiles)
        {
            if (!tableau.Cards.Any()) continue;

            var topCard = tableau.TopCard;
            foreach (var foundation in state.FoundationPiles)
            {
                if (foundation.CanAddCard(topCard))
                {
                    validMoves.Add(new SingleCardMove(tableau, foundation, topCard));
                }
            }
        }

        // Tableau → Tableau
        foreach (var tableau in state.TableauPiles)
        {
            if (!tableau.Cards.Any()) continue; // Skip empty tableau

            // Single Card Move
            var topCard = tableau.TopCard;
            foreach (var targetTableau in state.TableauPiles)
            {
                if (targetTableau.Index == tableau.Index) continue; // Skip the same tableau
                if (targetTableau.CanAddCard(topCard!))
                {
                    validMoves.Add(new SingleCardMove(tableau, targetTableau, topCard!));
                }
            }

            if (tableau.Cards.Count == 1) continue; // Skip if only one card


            var faceUp = tableau.Cards.Where(c => c.IsFaceUp).ToList();

            // All Multi card moves
            for (int i = 0; i < faceUp.Count - 1; i++)
            {
                var cardsToMove = faceUp.Skip(i).ToList();

                foreach (var targetTableau in state.TableauPiles)
                {

                    if (targetTableau.Index == tableau.Index) continue; // Skip the same tableau

                    if (cardsToMove.Count == 1 && targetTableau.CanAddCard(cardsToMove[0]))
                        validMoves.Add(new SingleCardMove(tableau, targetTableau, cardsToMove[0]));
                    else if (targetTableau.CanAddCards(cardsToMove))
                        validMoves.Add(new MultiCardMove(tableau, targetTableau, cardsToMove));
                }
            }
        }

        // Foundation → Tableau
        foreach (var foundation in state.FoundationPiles)
        {
            if (!foundation.Cards.Any()) continue;
            var topCard = foundation.TopCard;

            foreach (var targetTableau in state.TableauPiles)
            {
                if (targetTableau.CanAddCard(topCard))
                {
                    validMoves.Add(new SingleCardMove(foundation, targetTableau, topCard));
                }
            }
        }

        // Stock -> Waste (Cycling)
        if (!state.StockPile.IsEmpty)
        {
            validMoves.Add(state.CycleMove);
        }

        // Waste -> Stock (Recycle) // TODO: Maybe set a cap here?
        if (state.StockPile.IsEmpty && !state.WastePile.IsEmpty)
        {
            validMoves.Add(new MultiCardMove(state.WastePile, state.StockPile, state.WastePile.Cards));
        }

        return validMoves;
    }
}