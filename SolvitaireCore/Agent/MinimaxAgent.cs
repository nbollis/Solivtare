namespace SolvitaireCore;

public class MinimaxAgent<TGameState, TMove> : BaseAgent<TGameState, TMove>, ISearchAgent<TGameState, TMove>
    where TGameState : ITwoPlayerGameState<TMove>
    where TMove : IMove
{
    protected readonly Random Rand; 
    
    public int MaxDepth { get; set; }
    public StateEvaluator<TGameState, TMove> Evaluator { get; init; }
    public MinimaxAgent(StateEvaluator<TGameState, TMove> evaluator, int maxDepth = 6)
    {
        Evaluator = evaluator;
        MaxDepth = maxDepth;
        Rand = new();
    }

    public override string Name => "Minimax Agent";

    public override TMove GetNextAction(TGameState gameState, CancellationToken? cancellationToken = null)
    {
        var legalMoves = gameState.GetLegalMoves();
        if (legalMoves.Count == 0)
            throw new InvalidOperationException("No legal moves available.");

        int maximizingPlayer = gameState.CurrentPlayer; // Capture the player of interest
        List<ScoredMove> scoredMoves = ScoredMovePool.Get();
        List<ScoredMove> previousMoveOrder = ScoredMovePool.Get();

        try
        {
            // Iterative deepening: search from depth 1 up to MaxDepth  
            for (int depth = 1; depth <= MaxDepth; depth++)
            {
                if (cancellationToken is { IsCancellationRequested: false })
                    break;
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
                    if (cancellationToken is { IsCancellationRequested: true })
                        break;

                    // first move is made from perspective of maximizing player, always
                    gameState.ExecuteMove(moveInfo.Move);
                    var (minimaxScore, winDepth) = Minimax(gameState, depth - 1, double.NegativeInfinity, double.PositiveInfinity, false, maximizingPlayer, 1);
                    gameState.UndoMove(moveInfo.Move);

                    scoredMoves.Add(new ScoredMove(moveInfo.Move, moveInfo.MoveScore)
                    {
                        SearchScore = minimaxScore,
                        WinDepth = winDepth
                    });
                }

                // If cancellation is requested, break out of the loop
                if (scoredMoves.Count == 0)
                    break;

                // Sort so the best move is always first
                scoredMoves.Sort(MoveComparer);

                // Early exit if a winning move is found
                if (Math.Abs(scoredMoves[0].SearchScore - Evaluator.MaximumScore) < 1e-8)
                    return scoredMoves[0].Move;

                // We have losing moves in the pool, so we can ignore them in the next iteration
                if (Math.Abs(scoredMoves[^1].SearchScore - (-Evaluator.MaximumScore)) < 1e-8)
                {
                    double minScore = scoredMoves.Min(m => m.SearchScore);
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

                // Otherwise prepare for the next, deeper, iteration. 
                // Sort scored moves by SearchScore, then by WinDepth, then by MoveScore
                scoredMoves.Sort(MoveComparer);

                // Update previousMoveOrder for the next depth
                previousMoveOrder.Clear();
                previousMoveOrder.AddRange(scoredMoves);
            }

            // After cancellation or completion, return the best move found so far
            if (previousMoveOrder.Count > 0)
                return GetBest(previousMoveOrder).Move;

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
        var clone = (TGameState)gameState.Clone();
        int evalPlayer = perspectivePlayer ?? clone.CurrentPlayer;
        bool isMaximizing = evalPlayer == clone.CurrentPlayer;

        clone.ExecuteMove(move);

        var (score, _) = Minimax(clone, MaxDepth - 1, double.NegativeInfinity, double.PositiveInfinity, isMaximizing, evalPlayer, 1);
        return score;
    }

    /// <summary>
    /// Minimax algorithm with alpha-beta pruning and transposition table.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="depth"></param>
    /// <param name="alpha"></param>
    /// <param name="beta"></param>
    /// <param name="maximizingPlayer"></param>
    /// <param name="maximizingPlayerId"></param>
    /// <param name="plies">Number of half moves made</param>
    /// <returns></returns>
    public (double Score, int Depth) Minimax(TGameState state, int depth, double alpha, double beta, bool maximizingPlayer,
        int maximizingPlayerId, int plies)
    {
        int stateHash = state.GetHashCode();

        // Transposition table lookup
        if (TranspositionTable.TryGetValue(stateHash, out var entry))
        {
            if (entry.Depth >= depth && entry.Alpha <= alpha && entry.Beta >= beta)
                return (entry.Score, plies);
        }

        if (depth == 0 || state.IsGameWon || state.IsGameLost || state.IsGameDraw)
        {
            double eval = Evaluator.EvaluateState(state, maximizingPlayerId);
            TranspositionTable[stateHash] = new TranspositionTableEntry
            {
                Score = eval,
                Depth = depth,
                Alpha = alpha,
                Beta = beta
            };
            return (eval, plies);
        }

        // Move ordering: speeds up alpha beta pruning.  
        double bestScore = maximizingPlayer ? double.NegativeInfinity : double.PositiveInfinity; 
        int bestDepth = int.MaxValue;
        var moves = state.GetLegalMoves();
        foreach (var (move, _) in Evaluator.OrderMoves(moves, state, maximizingPlayer))
        {
            state.ExecuteMove(move);

            // Inline the recursive call to avoid unnecessary tuple creation
            (double score, int winDepth) minimaxResult = Minimax(state, depth - 1, alpha, beta, !maximizingPlayer, maximizingPlayerId, plies + 1);
            double score = minimaxResult.score;
            int winDepth = minimaxResult.winDepth;

            state.UndoMove(move);

            // At each node, if two moves have the same score, the one with the lower winDepth is preferred.
            if (maximizingPlayer)
            {
                if (score > bestScore || (Math.Abs(score - bestScore) < 1e-8 && winDepth < bestDepth))
                {
                    bestScore = score;
                    bestDepth = winDepth;
                }
                if (bestScore > alpha)
                    alpha = bestScore;
            }
            else
            {
                if (score < bestScore || (Math.Abs(score - bestScore) < 1e-8 && winDepth < bestDepth))
                {
                    bestScore = score;
                    bestDepth = winDepth;
                }
                if (bestScore < beta)
                    beta = bestScore;
            }

            // Alpha-beta pruning
            if (alpha >= beta)
                break;
        }

        TranspositionTable[stateHash] = new TranspositionTableEntry
        {
            Score = bestScore,
            Depth = depth,
            Alpha = alpha,
            Beta = beta
        };

        return (bestScore, bestDepth);
    }
}
