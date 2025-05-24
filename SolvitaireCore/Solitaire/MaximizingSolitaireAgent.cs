namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class MaximizingSolitaireAgent(StateEvaluator<SolitaireGameState, SolitaireMove> evaluator, int maxLookahead = 10) : MaximizingAgent<SolitaireGameState, SolitaireMove>(evaluator, maxLookahead)
{
}