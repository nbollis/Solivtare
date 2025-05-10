using MathNet.Numerics.Statistics;
using SolvitaireCore;

namespace SolvitaireGenetics;

public class GeneticSolitaireEvaluator(SolitaireChromosome chromosome) : SolitaireEvaluator
{
    public override double Evaluate(SolitaireGameState state)
    {
        int legalMoveCount = state.GetLegalMoves().Count;
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
        score += chromosome.GetWeight(SolitaireChromosome.LegalMoveWeightName) * legalMoveCount;
        score += chromosome.GetWeight(SolitaireChromosome.FoundationWeightName) * foundationCount;
        score += chromosome.GetWeight(SolitaireChromosome.WasteWeightName) * wasteCount;
        score += chromosome.GetWeight(SolitaireChromosome.StockWeightName) * stockCount;
        score += chromosome.GetWeight(SolitaireChromosome.CycleWeightName) * cycleCount;
        score += chromosome.GetWeight(SolitaireChromosome.EmptyTableauWeightName) * emptyTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.FaceUpTableauWeightName) * faceUpTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.FaceDownTableauWeightName) * faceDownTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.ConsecutiveFaceUpTableauWeightName) * consecutiveFaceUpTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.FaceUpBottomCardTableauWeightName) * faceUpBottomCardTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.KingIsBottomCardTableauWeightName) * kingIsBottomCardTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.AceInTableauWeightName) * aceInTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.FoundationRangeWeight) * foundationRange;
        score += chromosome.GetWeight(SolitaireChromosome.FoundationDeviationWeight) * foundationDeviation;

        return score;
    }

    public override bool ShouldSkipGame(SolitaireGameState state)
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
        score += chromosome.GetWeight(SolitaireChromosome.Skip_LegalMoveCount) * legalMoveCount;
        score += chromosome.GetWeight(SolitaireChromosome.Skip_FoundationCount) * foundationCount;
        score += chromosome.GetWeight(SolitaireChromosome.Skip_TopWasteIsUseful) * isWasteUseful;
        score += chromosome.GetWeight(SolitaireChromosome.Skip_WasteWeight) * wasteCount;
        score += chromosome.GetWeight(SolitaireChromosome.Skip_StockWeight) * stockCount;
        score += chromosome.GetWeight(SolitaireChromosome.Skip_CycleWeight) * cycleCount;
        score += chromosome.GetWeight(SolitaireChromosome.Skip_EmptyTableauCount) * emptyTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.Skip_FaceUpTableauCount) * faceUpTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.Skip_FaceDownTableauCount) * faceDownTableauCount;

        // Multiply by the number of moves made so far and its weight
        score *= chromosome.GetWeight(SolitaireChromosome.MoveCountScalarName) * state.MovesMade;

        double threshold = chromosome.GetWeight(SolitaireChromosome.Skip_ThresholdWeightName);
        return score < threshold;
    }
}