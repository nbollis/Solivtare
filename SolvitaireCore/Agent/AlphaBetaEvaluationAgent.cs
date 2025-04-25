namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class AlphaBetaEvaluationAgent(SolitaireEvaluator evaluator, int maxLookahead = 10) : ISolitaireAgent
{
    public string Name => "AlphaBeta Agent";
    public int LookAheadSteps { get; } = maxLookahead;

    public SolitaireMove GetNextMove(SolitaireGameState gameState)
    {
        SolitaireMove bestMove = null;

        // Iterative Deepening: Search for best of depth 1 and use that to determine the order to search depth 2 and so on. 
        // You would think this would make the search slower, but alpha-beta gains far outweigh. 
        for (int depth = 1; depth <= LookAheadSteps; depth++)
        {
            double alpha = double.NegativeInfinity;
            double beta = double.PositiveInfinity;

            foreach (var move in OrderMoves(gameState, gameState.GetLegalMoves()))
            {
                gameState.ExecuteMove(move);
                double score = EvaluateWithLookahead(gameState, depth - 1, alpha, beta, false);
                gameState.UndoMove(move);

                if (score > alpha)
                {
                    alpha = score;
                    bestMove = move;
                }
            }
        }


        return bestMove ?? throw new InvalidOperationException("No valid moves available.");
    }

    private IEnumerable<SolitaireMove> OrderMoves(SolitaireGameState gameState, IEnumerable<SolitaireMove> moves)
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