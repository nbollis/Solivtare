using SolvitaireCore.ConnectFour;

namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class MaximizingSolitaireAgent(StateEvaluator<SolitaireGameState, SolitaireMove> evaluator, int maxLookahead = 10) : MaximizingAgent<SolitaireGameState, SolitaireMove>(evaluator, maxLookahead)
{
    private static readonly ListPool<(SolitaireMove move, double score)> _scoredMovePool = new(8);
    public override IEnumerable<SolitaireMove> OrderMovesForEvaluation(SolitaireGameState gameState, List<SolitaireMove> moves)
    {
        // Get a pooled list for scored moves
        var scoredMoves = _scoredMovePool.Get();
        try
        {
            foreach (var move in moves)
            {
                double score = 0;
                if (PreviousBestMove != null && move.Equals(PreviousBestMove))
                    score = int.MaxValue;
                else
                {
                    if (move.ToPileIndex == SolitaireGameState.FoundationStartIndex) score += 20;
                    if (move.ToPileIndex <= SolitaireGameState.TableauEndIndex && move.FromPileIndex > SolitaireGameState.TableauEndIndex) score += 10;
                    if (move.FromPileIndex == SolitaireGameState.StockIndex) score += 2;
                }
                scoredMoves.Add((move, score));
            }

            scoredMoves.Sort((a, b) => b.score.CompareTo(a.score));

            foreach (var move in scoredMoves.Select(p => p.move))
            {
                yield return move;
            }
        }
        finally
        {
            _scoredMovePool.Return(scoredMoves);
        }
    }
}