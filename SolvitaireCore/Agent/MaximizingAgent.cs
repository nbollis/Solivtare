namespace SolvitaireCore;

/// <summary>
/// Picks the move which maximizes the score of the game state.
/// Best for one player games. 
/// </summary>
/// <param name="evaluator"></param>
/// <param name="maxLookahead">Moves to look ahead</param>
public class MaximizingAgent<TGameState, TMove>(StateEvaluator<TGameState, TMove> evaluator, int maxLookahead = 10) : BaseAgent<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    protected TMove? PreviousBestMove;

    public override string Name => "Maximizing Agent";
    public int LookAheadSteps { get; } = maxLookahead;
    public override TMove GetNextAction(TGameState gameState)
    {
        var moves = gameState.GetLegalMoves();
        TMove bestMove = moves[0]; // Fallback value

        // Iterative Deepening: Search for best of depth 1 and use that to determine the order to search depth 2 and so on.  
        // You would think this would make the search slower, but alpha-beta gains far outweigh.  
        for (int depth = 1; depth <= LookAheadSteps; depth++)
        {
            double alpha = double.NegativeInfinity;
            List<TMove> bestMoves = new();

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

    protected double EvaluateWithLookahead(TGameState gameState, int depth, double alpha, int moveCount)
    {
        // Generate a hash for the current game state
        int stateHash = gameState.GetHashCode();

        // Check if the state is already in the transposition table
        if (TranspositionTable.TryGetValue(stateHash, out var entry))
        {
            // If the stored depth is greater than or equal to the current depth, use the cached score
            if (entry.Depth >= depth && entry.Alpha <= alpha)
            {
                return entry.Score;
            }
        }

        // Base case: If depth is 0 or the game is over, evaluate the current state
        if (depth == 0 || gameState.IsGameWon || gameState.IsGameLost)
        {
            double score = evaluator.EvaluateState(gameState, moveCount);
            TranspositionTable[stateHash] = new TranspositionTableEntry
            {
                Score = score,
                Depth = depth,
                Alpha = alpha
            };
            return score;
        }

        // Recursive case: EvaluateState moves - Order moves to improve pruning
        double bestScore = double.NegativeInfinity;
        var moves = gameState.GetLegalMoves();
        foreach (var move in OrderMovesForEvaluation(gameState, moves))
        {
            gameState.ExecuteMove(move);
            double eval = EvaluateWithLookahead(gameState, depth - 1, alpha, moves.Count);
            gameState.UndoMove(move);

            bestScore = Math.Max(bestScore, eval);
            alpha = Math.Max(alpha, eval);

            // Prune the branch if the score cannot improve further
            if (eval <= alpha)
            {
                break; // Prune the branch
            }
        }

        // Store the result in the transposition table
        TranspositionTable[stateHash] = new TranspositionTableEntry
        {
            Score = bestScore,
            Depth = depth,
            Alpha = alpha
        };

        return bestScore;
    }

    public virtual IEnumerable<TMove> OrderMovesForEvaluation(TGameState gameState, List<TMove> moves)
    {
        // Order moves based on some heuristic
        // This is a placeholder implementation and should be replaced with actual logic
        return moves;
    }
}