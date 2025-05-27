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

        List<ScoredMove> scoredMoves = ScoredMovePool.Get();
        List<ScoredMove> previousMoveOrder = ScoredMovePool.Get();

        try
        {
            // Iterative deepening: search from depth 1 up to MaxDepth  
            for (int depth = 1; depth <= MaxDepth; depth++)
            {
                scoredMoves.Clear();

                // Order moves based on static evaluation for depth 1, or use previous order for deeper searches
                List<ScoredMove> moveInfos = depth == 1
                    ? legalMoves
                        .Select(move => new ScoredMove(move, Evaluator.EvaluateMove(gameState, move)))
                        .OrderByDescending(m => m.MoveScore)
                        .ToList()
                    : previousMoveOrder;

                foreach (var moveInfo in moveInfos)
                {
                    double score;
                    if (moveInfo.Move.IsTerminatingMove) // Evaluate as a leaf node, do not recurse
                    {
                        score = Evaluator.EvaluateMove(gameState, moveInfo.Move);
                    }
                    else // Recurse to evaluate the move
                    {
                        gameState.ExecuteMove(moveInfo.Move);
                        score = Maximize(gameState, depth - 1);
                        gameState.UndoMove(moveInfo.Move);
                    }

                    scoredMoves.Add(new ScoredMove(moveInfo.Move, moveInfo.MoveScore)
                    {
                        SearchScore = score,
                        WinDepth = depth
                    });
                }

                double maxScore = scoredMoves.Max(m => m.SearchScore);
                double minScore = scoredMoves.Min(m => m.SearchScore);

                // If we found a winning move, return it immediately
                if (Math.Abs(maxScore - Evaluator.MaximumScore) < 1e-8)
                    return scoredMoves.First(m => Math.Abs(m.SearchScore - maxScore) < 1e-8).Move;
                
                // We have losing moves in the pool, so we can ignore them in the next iteration
                if (Math.Abs(minScore - (-Evaluator.MaximumScore)) < 1e-8)
                {
                    var worstMoves = scoredMoves.Where(m => Math.Abs(m.SearchScore - minScore) < 1e-8).ToList();

                    // All moves lose, just pick one and get it over with. 
                    if (worstMoves.Count == scoredMoves.Count)
                        return scoredMoves[Rand.Next(scoredMoves.Count)].Move;

                    int slowestLoss = worstMoves.Max(m => m.WinDepth);
                    worstMoves = worstMoves.Where(m => m.WinDepth == slowestLoss).ToList();

                    // Remove the worst moves from consideration for the next rounds  
                    scoredMoves.RemoveAll(m => worstMoves.Contains(m));

                    // After removing all losing moves, only one remained, send it. 
                    if (scoredMoves.Count == 1)
                        return scoredMoves[0].Move;
                }

                // If we reach the maximum depth, we want to select the best move based on SearchScore, WinDepth, and MoveScore
                if (depth == MaxDepth)
                {
                    // Find the best minimax score  
                    var bestMoves = scoredMoves.Where(m => Math.Abs(m.SearchScore - maxScore) < 1e-8).ToList();

                    // Tiebreak: prefer lowest winDepth (fastest win)
                    int bestDepth = bestMoves.Min(m => m.WinDepth);
                    bestMoves = bestMoves.Where(m => m.WinDepth == bestDepth).ToList();

                    // Further tiebreak: moveScore
                    if (bestMoves.Count > 1)
                    {
                        double bestMoveScore = bestMoves.Max(m => m.MoveScore);
                        bestMoves = bestMoves.Where(m => Math.Abs(m.MoveScore - bestMoveScore) < 1e-8).ToList();
                    }

                    if (bestMoves.Count == 1)
                        return bestMoves[0].Move;

                    // If there's still a tie, randomly select one of the best moves
                    return bestMoves[Rand.Next(bestMoves.Count)].Move;
                }

                // Otherwise prepare for the next, deeper, iteration. 
                // Sort scored moves by SearchScore, then by WinDepth, then by MoveScore
                scoredMoves.Sort(MoveComparer);

                // Update previousMoveOrder for the next depth
                previousMoveOrder.Clear();
                previousMoveOrder.AddRange(scoredMoves);
            }

            // Fallback in case no move is selected  
            return legalMoves[0];
        }
        finally
        {
            ScoredMovePool.Return(scoredMoves);
            ScoredMovePool.Return(previousMoveOrder);
        }
    }

    public override double EvaluateMoveWithAgent(TGameState gameState, TMove move, int? perspectivePlayer = null)
    {
        gameState.ExecuteMove(move);
        double score = Maximize(gameState, MaxDepth - 1);
        gameState.UndoMove(move);
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
            state.ExecuteMove(move);
            double score = Maximize(state, depth - 1);
            state.UndoMove(move); 

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
