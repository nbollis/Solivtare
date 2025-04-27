namespace SolvitaireCore;

public class GeneticSolitaireEvaluator(SolitaireChromosome chromosome) : SolitaireEvaluator
{
    public override double Evaluate(SolitaireGameState state)
    {
        int legalMoveCount = state.GetLegalMoves().Count();
        int foundationCount = state.FoundationPiles.Sum(pile => pile.Count);
        int wasteCount = state.WastePile.Count;
        int stockCount = state.StockPile.Count;
        int cycleCount = state.CycleCount;

        int emptyTableauCount = state.TableauPiles.Count(pile => pile.IsEmpty);
        int faceUpTableauCount = 0;
        int faceDownTableauCount = 0;
        int consecutiveFaceUpTableauCount = 0;
        int faceUpBottomCardTableaCount = 0;
        int kingIsBottomCardTableauCount = 0;
        int aceInTableauCount = 0;

        foreach (var tableau in state.TableauPiles)
        {
            int faceDownCount = tableau.Cards.TakeWhile(card => !card.IsFaceUp).Count();
            faceUpTableauCount += faceDownCount;

            faceUpTableauCount += tableau.Count - faceDownCount;

            if (tableau.BottomCard.IsFaceUp) // reward bottom card of pile exposed. 
            {
                faceUpBottomCardTableaCount++;
                if (tableau.BottomCard.Rank == Rank.King) // extra reward that card being a king
                    kingIsBottomCardTableauCount++;
            }

            for (int i = tableau.Cards.Count - 1; i > 0; i--)
            {
                if (tableau.Cards[i].Rank == Rank.Ace)
                    aceInTableauCount++;

                // Check if the current card is a valid continuation of the sequence
                if (tableau.Cards[i].Color != tableau.Cards[i - 1].Color &&
                    tableau.Cards[i].Rank == tableau.Cards[i - 1].Rank - 1)
                {
                    consecutiveFaceUpTableauCount++;
                }
                else
                {
                    break;
                }
            }
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
        score += chromosome.GetWeight(SolitaireChromosome.FaceUpBottomCardTableauWeightName) * faceUpBottomCardTableaCount;
        score += chromosome.GetWeight(SolitaireChromosome.KingIsBottomCardTableauWeightName) * kingIsBottomCardTableauCount;
        score += chromosome.GetWeight(SolitaireChromosome.AceInTableauWeightName) * aceInTableauCount;

        return score;
    }
}