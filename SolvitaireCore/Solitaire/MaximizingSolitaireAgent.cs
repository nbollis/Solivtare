namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class MaximizingSolitaireAgent(SolitaireEvaluator evaluator, int maxLookahead = 10) : MaximizingAgent<SolitaireGameState, SolitaireMove>(evaluator, maxLookahead)
{
    public override SolitaireMove GetNextAction(SolitaireGameState gameState)
    {
        SolitaireMove bestMove = null!;

        var moves = gameState.GetLegalMoves();
        if (evaluator.ShouldSkipGame(gameState)) // TODO: Some better criteria for skipping games
        {
            return new SkipGameMove();
        }

        // Iterative Deepening: Search for best of depth 1 and use that to determine the order to search depth 2 and so on.  
        // You would think this would make the search slower, but alpha-beta gains far outweigh.  
        for (int depth = 1; depth <= LookAheadSteps; depth++)
        {
            double alpha = double.NegativeInfinity;
            List<SolitaireMove> bestMoves = new();

            foreach (var move in OrderMovesForEvaluation(gameState, moves))
            {
                gameState.ExecuteMove(move);
                double score = EvaluateWithLookahead(gameState, depth - 1, alpha, moves.Count);
                gameState.UndoMove(move);

                if (score > alpha)
                {
                    alpha = score;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                else if (Math.Abs(score - alpha) < 0.0000001)
                {
                    bestMoves.Add(move);
                }
            }

            // Pick one move at random from the best moves  
            if (bestMoves.Count > 0)
            {
                Random random = new();
                bestMove = bestMoves[random.Next(bestMoves.Count)];
            }

            // Cache the best move for the next depth  
            PreviousBestMove = bestMove;
        }


        return bestMove;
    }

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