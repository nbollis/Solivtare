namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class BruteForceEvaluationAgent(SolitaireEvaluator evaluator, int maxLookahead = 10) 
    : SolitaireAgent
{
    public override string Name => "Brute Force Agent";
    public int LookAheadSteps { get; } = maxLookahead;

    public override SolitaireMove GetNextMove(SolitaireGameState gameState)
    {
        SolitaireMove bestMove = null;
        double bestScore = double.NegativeInfinity;

        foreach (var move in gameState.GetLegalMoves())
        {
            gameState.ExecuteMove(move);
            double score = EvaluateWithLookahead(gameState, LookAheadSteps - 1);
            gameState.UndoMove(move);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove ?? throw new InvalidOperationException("No valid moves available.");
    }

    private double EvaluateWithLookahead(SolitaireGameState gameState, int depth)
    {
        if (depth == 0 || gameState.IsGameWon || gameState.IsGameLost)
        {
            return evaluator.Evaluate(gameState);
        }

        double bestScore = double.NegativeInfinity;

        foreach (var move in gameState.GetLegalMoves())
        {
            gameState.ExecuteMove(move);
            double score = EvaluateWithLookahead(gameState, depth - 1);
            gameState.UndoMove(move);
            if (score > bestScore)
            {
                bestScore = score;
            }
        }

        return bestScore;
    }
}