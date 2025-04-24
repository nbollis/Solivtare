namespace SolvitaireCore;

/// <summary>
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.
/// </summary>
public class SimpleEvaluationAgent(SolitaireEvaluator evaluator) : ISolitaireAgent
{
    public string Name => "Greedy Agent";
    public int LookAheadSteps;

    public ISolitaireMove GetNextMove(SolitaireGameState gameState)
    {
        ISolitaireMove bestMove = null;
        double bestScore = double.NegativeInfinity;

        foreach (var move in gameState.GetLegalMoves())
        {
            gameState.ExecuteMove(move);
            double score = evaluator.Evaluate(gameState);
            gameState.UndoMove(move);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }
}