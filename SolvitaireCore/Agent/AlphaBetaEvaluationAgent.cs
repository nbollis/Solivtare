namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class AlphaBetaEvaluationAgent(SolitaireEvaluator evaluator, int maxLookahead = 20) : ISolitaireAgent
{
    public string Name => "AlphaBeta Agent";
    public int LookAheadSteps { get; } = maxLookahead;

    public ISolitaireMove GetNextMove(SolitaireGameState gameState)
    {
        ISolitaireMove bestMove = null;
        double bestScore = double.NegativeInfinity;
        double alpha = double.NegativeInfinity;
        double beta = double.PositiveInfinity;

        var orderedMoves = OrderMoves(gameState, gameState.GetLegalMoves());
        foreach (var move in orderedMoves)
        {
            gameState.ExecuteMove(move);
            double score = EvaluateWithLookahead(gameState, LookAheadSteps - 1, alpha, beta, false);
            gameState.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            alpha = Math.Max(alpha, bestScore);

            // Prune the branch if alpha is greater than or equal to beta
            if (beta <= alpha)
            {
                break;
            }
        }


        return bestMove ?? throw new InvalidOperationException("No valid moves available.");
    }

    private IEnumerable<ISolitaireMove> OrderMoves(SolitaireGameState gameState, IEnumerable<ISolitaireMove> moves)
    {
        return moves.OrderByDescending(move =>
        {
            gameState.ExecuteMove(move);
            double score = evaluator.Evaluate(gameState);
            gameState.UndoMove(move);
            return score;
        });
    }

    private double EvaluateWithLookahead(SolitaireGameState gameState, int depth, double alpha, double beta, bool isMaximizingPlayer)
    {
        // Base case: If depth is 0 or the game is over, evaluate the current state
        if (depth == 0 || gameState.IsGameWon || gameState.IsGameLost)
        {
            return evaluator.Evaluate(gameState);
        }

        if (isMaximizingPlayer)
        {
            double maxEval = double.NegativeInfinity;

            foreach (var move in gameState.GetLegalMoves())
            {
                gameState.ExecuteMove(move);
                double eval = EvaluateWithLookahead(gameState, depth - 1, alpha, beta, false);
                gameState.UndoMove(move);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                // Prune the branch if alpha is greater than or equal to beta
                if (beta <= alpha)
                {
                    break;
                }
            }

            return maxEval;
        }
        else
        {
            double minEval = double.PositiveInfinity;

            foreach (var move in gameState.GetLegalMoves())
            {
                gameState.ExecuteMove(move);
                double eval = EvaluateWithLookahead(gameState, depth - 1, alpha, beta, true);
                gameState.UndoMove(move);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                // Prune the branch if alpha is greater than or equal to beta
                if (beta <= alpha)
                {
                    break;
                }
            }

            return minEval;
        }
    }
}