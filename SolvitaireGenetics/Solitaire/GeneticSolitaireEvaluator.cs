using MathNet.Numerics.Statistics;
using SolvitaireCore;

namespace SolvitaireGenetics;

public class GeneticSolitaireEvaluator : SolitaireEvaluator
{
    private readonly SolitaireChromosome _chromosome;

    public GeneticSolitaireEvaluator(SolitaireChromosome chromosome)
    {
        _chromosome = chromosome;
        MaximumScore = int.MaxValue;
    }

    public override double EvaluateMove(SolitaireGameState state, SolitaireMove move)
    {
        if (move.IsTerminatingMove)
        {
            // Reuse skip evaluation logic
            return EvaluateSkipScore(state);
        }

        double score = 0;

        // Foundation moves are mutually exclusive
        if (move.ToPileIndex >= SolitaireGameState.FoundationStartIndex && move.ToPileIndex <= SolitaireGameState.FoundationEndIndex) 
            score += _chromosome.GetWeight(SolitaireChromosome.Move_ToFoundationWeightName);
        else if (move.FromPileIndex >= SolitaireGameState.FoundationStartIndex && move.ToPileIndex <= SolitaireGameState.FoundationEndIndex)
            score += _chromosome.GetWeight(SolitaireChromosome.Move_FromFoundationWeightName);

        // Tableau moves
        if (move.ToPileIndex <= SolitaireGameState.TableauEndIndex)
        {
            score += _chromosome.GetWeight(SolitaireChromosome.Move_ToTableauWeightName);
            if (move.FromPileIndex > SolitaireGameState.TableauEndIndex)
                score += _chromosome.GetWeight(SolitaireChromosome.Move_TableaToTableauWeightName);
        }
        else if (move.FromPileIndex <= SolitaireGameState.TableauEndIndex)
            score += _chromosome.GetWeight(SolitaireChromosome.Move_FromTableauWeightName);

        // Waste moves
        if (move.FromPileIndex == SolitaireGameState.WasteIndex)
            score += _chromosome.GetWeight(SolitaireChromosome.Move_FromWasteWeightName);

        // Stock moves
        if (move.FromPileIndex == SolitaireGameState.StockIndex)
            score += _chromosome.GetWeight(SolitaireChromosome.Move_FromStockWeightName);

        return score;
    }

    public override double EvaluateState(SolitaireGameState state, int? moveCount = null)
    {
        if (state.IsGameWon)
            return MaximumScore;
        int legalMoveCount = moveCount ?? state.GetLegalMoves().Count;
        int foundationCount = state.FoundationPiles.Sum(pile => pile.Count);
        double foundationRange = state.FoundationPiles.Max(pile => pile.Count) - state.FoundationPiles.Min(pile => pile.Count);
        double foundationDeviation = state.FoundationPiles.Select(p => p.TopCard != null ? (double)p.TopCard.Rank : 0.0).StandardDeviation();
        int wasteCount = state.WastePile.Count;
        int stockCount = state.StockPile.Count;
        int cycleCount = state.CycleCount;

        int emptyTableauCount = state.TableauPiles.Count(pile => pile.IsEmpty);
        int faceUpTableauCount = 0;
        int faceDownTableauCount = 0;
        int consecutiveFaceUpTableauCount = 0;
        int faceUpBottomCardTableauCount = 0;
        int kingIsBottomCardTableauCount = 0;
        int aceInTableauCount = 0;

        foreach (var tableau in state.TableauPiles)
        {
            int faceDownCount = 0;
            int consecutiveFaceUpCount = 0;
            bool bottomCardIsFaceUp = tableau.BottomCard?.IsFaceUp ?? false;

            for (int i = 0; i < tableau.Cards.Count; i++)
            {
                var card = tableau.Cards[i];
                if (!card.IsFaceUp)
                {
                    faceDownCount++;
                }
                else
                {
                    if (i == tableau.Cards.Count - 1 && bottomCardIsFaceUp)
                    {
                        faceUpBottomCardTableauCount++;
                        if (card.Rank == Rank.King)
                            kingIsBottomCardTableauCount++;
                    }

                    if (card.Rank == Rank.Ace)
                        aceInTableauCount++;

                    if (i > 0)
                    {
                        var prevCard = tableau.Cards[i - 1];
                        if (card.Color != prevCard.Color && card.Rank == prevCard.Rank - 1)
                        {
                            consecutiveFaceUpCount++;
                        }
                        else
                        {
                            consecutiveFaceUpCount = 0;
                        }
                    }
                }
            }

            faceUpTableauCount += tableau.Cards.Count - faceDownCount;
            consecutiveFaceUpTableauCount += consecutiveFaceUpCount;
            faceDownTableauCount += faceDownCount;
        }

        // Now combine everything using the chromosome weights: 
        double score = 0.0;
        score += _chromosome.GetWeight(SolitaireChromosome.LegalMoveWeightName) * legalMoveCount;
        score += _chromosome.GetWeight(SolitaireChromosome.FoundationWeightName) * foundationCount;
        score += _chromosome.GetWeight(SolitaireChromosome.WasteWeightName) * wasteCount;
        score += _chromosome.GetWeight(SolitaireChromosome.StockWeightName) * stockCount;
        score += _chromosome.GetWeight(SolitaireChromosome.CycleWeightName) * cycleCount;
        score += _chromosome.GetWeight(SolitaireChromosome.EmptyTableauWeightName) * emptyTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.FaceUpTableauWeightName) * faceUpTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.FaceDownTableauWeightName) * faceDownTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.ConsecutiveFaceUpTableauWeightName) * consecutiveFaceUpTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.FaceUpBottomCardTableauWeightName) * faceUpBottomCardTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.KingIsBottomCardTableauWeightName) * kingIsBottomCardTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.AceInTableauWeightName) * aceInTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.FoundationRangeWeightName) * foundationRange;
        score += _chromosome.GetWeight(SolitaireChromosome.FoundationDeviationWeightName) * foundationDeviation;

        return score;
    }

    public override double EvaluateSkipScore(SolitaireGameState state)
    {
        double score = 0.0;

        // Parse out data
        var moves = state.GetLegalMoves();
        int legalMoveCount = moves.Count;
        int foundationCount = state.FoundationPiles.Sum(pile => pile.Count);

        int wasteCount = state.WastePile.Count;
        int stockCount = state.StockPile.Count;
        int cycleCount = state.CycleCount;
        int emptyTableauCount = state.TableauPiles.Count(pile => pile.IsEmpty);
        int faceUpTableauCount = state.TableauPiles.Sum(pile => pile.Cards.Count(card => card.IsFaceUp));
        int faceDownTableauCount = state.TableauPiles.Sum(pile => pile.Cards.Count(card => !card.IsFaceUp));
        int isWasteUseful = moves.Any(p => p.FromPileIndex == SolitaireGameState.WasteIndex) ? 1 : 0;

        // Sum up score contributions
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_LegalMoveCount) * legalMoveCount;
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_FoundationCount) * foundationCount;
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_TopWasteIsUseful) * isWasteUseful;
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_WasteWeight) * wasteCount;
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_StockWeight) * stockCount;
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_CycleWeight) * cycleCount;
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_EmptyTableauCount) * emptyTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_FaceUpTableauCount) * faceUpTableauCount;
        score += _chromosome.GetWeight(SolitaireChromosome.Skip_FaceDownTableauCount) * faceDownTableauCount;

        //// Multiply by the number of moves made so far and its weight
        //score *= chromosome.GetWeight(SolitaireChromosome.MoveCountScalarName) * state.MovesMade;
        return score;
    }
}