using System.Net.NetworkInformation;

namespace SolivtaireCore;

/// <summary>
/// Represents a move made by a player
/// </summary>
public interface IMove
{
    bool IsValid(GameState state);
    void Execute(GameState state);
}

public interface IMoveGenerator
{
    IEnumerable<IMove> GenerateMoves(GameState state);
}

public class SolitaireMoveGenerator : IMoveGenerator
{
    public IEnumerable<IMove> GenerateMoves(GameState state)
    {
        var validMoves = new List<IMove>();

        // Waste -> XX
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

        // Generate moves for Stock and Waste piles
        if (!state.StockPile.IsEmpty)
        {
            validMoves.Add(state.CycleMove);
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

        // Generate moves for Foundation piles
        foreach (var foundationPile in state.FoundationPiles)
        {
            if (!foundationPile.IsEmpty)
            {
                foreach (var card in foundationPile.Cards)
                {
                    validMoves.Add(new SingleCardMove(foundationPile, state.WastePile, card));
                }
            }
        }
        return validMoves;
    }
}