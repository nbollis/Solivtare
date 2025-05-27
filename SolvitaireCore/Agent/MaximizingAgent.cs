namespace SolvitaireCore;

/// <summary>
/// Picks the move which maximizes the score of the game state.
/// Best for one player games. 
/// </summary>
public class MaximizingAgent<TGameState, TMove> : BaseAgent<TGameState, TMove>, ISearchAgent<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    protected readonly Random Rand;
    public int MaxDepth { get; set; }
    public StateEvaluator<TGameState, TMove> Evaluator { get; init; }

    public MaximizingAgent(StateEvaluator<TGameState, TMove> evaluator, int maxDepth = 3)
    {
        Evaluator = evaluator;
        MaxDepth = maxDepth;
        Rand = new();
    }

    public override string Name => "Maximizing Agent";
    public override TMove GetNextAction(TGameState gameState)
    {
        var legalMoves = gameState.GetLegalMoves();
        if (legalMoves.Count == 0)
            throw new InvalidOperationException("No legal moves available.");

        TMove bestMove = legalMoves[0];
        //var debugInfo = new List<(TMove Move, double Score, int Depth)>(); // Store moves and evaluations for debugging  

        // Iterative deepening: search from depth 1 up to MaxDepth  
        for (int depth = 1; depth <= MaxDepth; depth++)
        {
            double bestScore = double.NegativeInfinity;
            List<TMove> bestMoves = new();

            foreach (var move in legalMoves)
            {
                var clone = (TGameState)gameState.Clone();
                clone.ExecuteMove(move);

                double score;
                if (move.IsTerminatingMove)
                {
                    // Evaluate as a leaf node, do not recurse
                    score = Evaluator.EvaluateMove(gameState, move);
                }
                else
                {
                    score = Maximize(clone, depth - 1);
                }

                //debugInfo.Add((move, score, depth)); // Add move, score, and depth to debug info  

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                else if (Math.Abs(score - bestScore) < 1e-8)
                {
                    bestMoves.Add(move);
                }
            }

            if (bestMoves.Count > 0)
                bestMove = bestMoves[Rand.Next(bestMoves.Count)];
        }

        //debugInfo = debugInfo.OrderBy(p => p.Move.ToString()).ThenBy(p => p.Depth).ToList();
        // Debugging: Place a breakpoint here to inspect debugInfo  
        return bestMove;
    }

    public override double EvaluateMoveWithAgent(TGameState gameState, TMove move, int? perspectivePlayer = null)
    {
        // Clone state to avoid mutating the real game
        var clone = (TGameState)gameState.Clone();
        clone.ExecuteMove(move);

        // Use the same depth as the agent's current setting
        var score = Maximize(clone, MaxDepth);
        return score;
    }

    private double Maximize(TGameState state, int depth)
    {
        int stateHash = state.GetHashCode();

        // Check transposition table
        if (TranspositionTable.TryGetValue(stateHash, out var entry))
        {
            // If the stored entry is at least as deep, use it
            if (entry.Depth >= depth)
                return entry.Score;
        }

        if (depth == 0 || state.IsGameWon || state.IsGameLost)
        {
            double eval = Evaluator.EvaluateState(state);
            // Store in transposition table
            TranspositionTable[stateHash] = new TranspositionTableEntry
            {
                Score = eval,
                Depth = depth
            };
            return eval;
        }

        var moves = state.GetLegalMoves();
        if (moves.Count == 0)
        {
            double eval = Evaluator.EvaluateState(state);
            TranspositionTable[stateHash] = new TranspositionTableEntry
            {
                Score = eval,
                Depth = depth
            };
            return eval;
        }

        double bestScore = double.NegativeInfinity;
        foreach (var move in moves)
        {
            var clone = (TGameState)state.Clone();
            clone.ExecuteMove(move);

            double moveScore = Evaluator.EvaluateMove(state, move);
            double score = moveScore;

            if (!move.IsTerminatingMove)
                score += Maximize(clone, depth - 1);

            if (score > bestScore)
                bestScore = score;
        }

        // Store the result in the transposition table
        TranspositionTable[stateHash] = new TranspositionTableEntry
        {
            Score = bestScore,
            Depth = depth
        };

        return bestScore;
    }
}
