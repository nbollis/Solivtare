
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
        if (IsGameUnwinnable(gameState)) // TODO: Some better criteria for skipping games
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
                double score = EvaluateWithLookahead(gameState, depth - 1, alpha);
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
        // Advanced unwinnability logic: Check if the evaluation score is below a threshold  
        double evaluationScore = evaluator.Evaluate(gameState);
        double bestScore = double.NegativeInfinity;

        double alpha = double.NegativeInfinity;
        double beta = double.PositiveInfinity;

        Dictionary<SolitaireMove, double> moveScores = new();

        foreach (var move in OrderMoves(gameState, gameState.GetLegalMoves()))
        {
            gameState.ExecuteMove(move);
            double score = EvaluateWithLookahead(gameState, LookAheadSteps - 1, alpha);
            gameState.UndoMove(move);

            if (moveScores.TryGetValue(move, out var previousScore) && score > previousScore)
                moveScores[move] = score;
            else
                moveScores.Add(move, score);

            if (score > alpha)
            {
                alpha = score;
                bestScore = score;
            }
        }

        // Check if all moves lead to a score below the unwinnable threshold  
        var firstScore = moveScores.First().Value;
        bool allScoresIdentical = moveScores.Values.All(score => Math.Abs(score - firstScore) <= 0.000000000001);
        bool noScoreImprovement = Math.Abs(bestScore - evaluationScore) <= 0.000001;

        return noScoreImprovement &&  (allScoresIdentical || base.IsGameUnwinnable(gameState));
    }

    private IEnumerable<SolitaireMove> OrderMoves(SolitaireGameState gameState, IEnumerable<SolitaireMove> moves)
    {
        return moves.OrderByDescending(move =>
        {
            if (_previousBestMove != null && move.Equals(_previousBestMove))
            {
                return int.MaxValue; // Highest priority
            }
            // Heuristic-based scoring for move ordering
            double score = 0;
            if (move.ToPileIndex == SolitaireGameState.FoundationStartIndex) score += 20; // Moves to foundation
            if (move.ToPileIndex <= SolitaireGameState.TableauEndIndex && move.FromPileIndex > SolitaireGameState.TableauEndIndex) score += 10; // Moves to tableau
            if (move.FromPileIndex == SolitaireGameState.StockIndex) score += 2; // Reduces stock pile

            score += gameState.EvaluateMove(move, evaluator);

            return score;
        });
    }

    private double EvaluateWithLookahead(SolitaireGameState gameState, int depth, double alpha)
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
            double score = evaluator.Evaluate(gameState);
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
        foreach (var move in OrderMoves(gameState, gameState.GetLegalMoves()))
        {
            gameState.ExecuteMove(move);
            double eval = EvaluateWithLookahead(gameState, depth - 1, alpha);
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
}