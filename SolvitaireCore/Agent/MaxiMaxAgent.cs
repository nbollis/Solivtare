
using System;

namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class MaxiMaxAgent(SolitaireEvaluator evaluator, int maxLookahead = 10) : SolitaireAgent
{
    
    private SolitaireMove? _previousBestMove;

    public override string Name => "MaxiMax Agent";
    public int LookAheadSteps { get; } = maxLookahead;

    public override AgentDecision GetNextAction(SolitaireGameState gameState)
    {
        SolitaireMove bestMove = null!;

        var moves = gameState.GetLegalMoves();
        if (evaluator.ShouldSkipGame(gameState)) // TODO: Some better criteria for skipping games
        {
            return AgentDecision.SkipGame();
        }

        // Iterative Deepening: Search for best of depth 1 and use that to determine the order to search depth 2 and so on.  
        // You would think this would make the search slower, but alpha-beta gains far outweigh.  
        for (int depth = 1; depth <= LookAheadSteps; depth++)
        {
            double alpha = double.NegativeInfinity;
            List<SolitaireMove> bestMoves = new();

            foreach (var move in OrderMoves(gameState, moves))
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
            _previousBestMove = bestMove;
        }


        return AgentDecision.PlayMove(bestMove);
    }

    public override bool IsGameUnwinnable(SolitaireGameState gameState)
    {
        return evaluator.ShouldSkipGame(gameState);
    }

    private double EvaluateWithLookahead(SolitaireGameState gameState, int depth, double alpha, int moveCount)
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
        if (depth == 0 || gameState.IsGameWon /*|| gameState.IsGameLost*/)
        {
            double score = evaluator.Evaluate(gameState, moveCount);
            TranspositionTable[stateHash] = new TranspositionTableEntry
            {
                Score = score,
                Depth = depth,
                Alpha = alpha
            };
            return score;
        }

        // Recursive case: Evaluate moves - Order moves to improve pruning
        double bestScore = double.NegativeInfinity;
        var moves = gameState.GetLegalMoves();
        foreach (var move in OrderMoves(gameState, moves))
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

    private static readonly ListPool<(SolitaireMove move, double score)> _scoredMovePool = new(8);

    private IEnumerable<SolitaireMove> OrderMoves(SolitaireGameState gameState, IEnumerable<SolitaireMove> moves)
    {
        // Get a pooled list for scored moves
        var scoredMoves = _scoredMovePool.Get();
        try
        {
            foreach (var move in moves)
            {
                double score = 0;
                if (_previousBestMove != null && move.Equals(_previousBestMove))
                    score = int.MaxValue;
                else
                {
                    if (move.ToPileIndex == SolitaireGameState.FoundationStartIndex) score += 20;
                    if (move.ToPileIndex <= SolitaireGameState.TableauEndIndex && move.FromPileIndex > SolitaireGameState.TableauEndIndex) score += 10;
                    if (move.FromPileIndex == SolitaireGameState.StockIndex) score += 2;
                    score += gameState.EvaluateMove(move, evaluator);
                }
                scoredMoves.Add((move, score));
            }

            // Sort once, then yield
            foreach (var (move, _) in scoredMoves.OrderByDescending(x => x.score))
                yield return move;
        }
        finally
        {
            _scoredMovePool.Return(scoredMoves);
        }
    }
}