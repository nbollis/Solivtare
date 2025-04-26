using static System.Formats.Asn1.AsnWriter;

namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class AlphaBetaEvaluationAgent(SolitaireEvaluator evaluator, int maxLookahead = 5) : SolitaireAgent
{
    private SolitaireMove? _previousBestMove;

    public override string Name => "AlphaBeta Agent";
    public int LookAheadSteps { get; } = maxLookahead;

    public override SolitaireMove GetNextMove(SolitaireGameState gameState)
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
                double score = EvaluateWithLookahead(gameState, depth - 1, alpha, beta);
                gameState.UndoMove(move);

                if (score > alpha)
                {
                    alpha = score;
                    bestMove = move;
                }
            }
            
            // Cache the best move for the next depth
            _previousBestMove = bestMove;
        }


        return bestMove ?? throw new InvalidOperationException("No valid moves available.");
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
            double score = EvaluateWithLookahead(gameState, LookAheadSteps - 1, alpha, beta);
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

    private int GetMovePriority(SolitaireMove move, SolitaireGameState gameState)
    {
        // Foundation moves (high priority)
        if (move.ToPileIndex >= SolitaireGameState.FoundationStartIndex &&
            move.ToPileIndex <= SolitaireGameState.FoundationEndIndex)
        {
            return 3;
        }

        // Moves that uncover hidden cards in the tableau (high priority)
        if (move.FromPileIndex >= SolitaireGameState.TableaStartIndex &&
            move.FromPileIndex <= SolitaireGameState.TableauEndIndex)
        {
            var fromPile = gameState.GetPileByIndex(move.FromPileIndex) as TableauPile;
            if (fromPile is { Cards.Count: > 1 } && !fromPile.Cards[^2].IsFaceUp)
            {
                return 2;
            }
        }

        // Tableau-to-Tableau moves (medium priority)
        if (move.FromPileIndex >= SolitaireGameState.TableaStartIndex &&
            move.FromPileIndex <= SolitaireGameState.TableauEndIndex &&
            move.ToPileIndex >= SolitaireGameState.TableaStartIndex &&
            move.ToPileIndex <= SolitaireGameState.TableauEndIndex)
        {
            return 1;
        }

        // Stock-to-Waste moves (low priority)
        if (move.FromPileIndex == SolitaireGameState.StockIndex &&
            move.ToPileIndex == SolitaireGameState.WasteIndex)
        {
            return 0;
        }

        // Default priority
        return 0;
    }

    private IEnumerable<SolitaireMove> OrderMoves(SolitaireGameState gameState, IEnumerable<SolitaireMove> moves)
    {
        return moves.OrderByDescending(move =>
        {
            // Prioritize the best move from the previous depth
            if (_previousBestMove != null && move.Equals(_previousBestMove))
            {
                return int.MaxValue; // Highest priority
            }

            // Use the static heuristic for other moves
            return GetMovePriority(move, gameState);
        });
    }

    private double EvaluateWithLookahead(SolitaireGameState gameState, int depth, double alpha, double beta)
    {
        // Generate a hash for the current game state
        int stateHash = gameState.GetHashCode();

        // Check if the state is already in the transposition table
        if (TranspositionTable.TryGetValue(stateHash, out var entry))
        {
            // If the stored depth is greater than or equal to the current depth, use the cached score
            if (entry.Depth >= depth)
            {
                if (entry.Alpha <= alpha && entry.Beta >= beta)
                {
                    return entry.Score;
                }
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
                Alpha = alpha,
                Beta = beta
            };
            return score;
        }

        double bestScore = double.NegativeInfinity;
        // Order moves to improve alpha-beta pruning
        foreach (var move in OrderMoves(gameState, gameState.GetLegalMoves()))
        {
            gameState.ExecuteMove(move);
            double eval = EvaluateWithLookahead(gameState, depth - 1, alpha, beta);
            gameState.UndoMove(move);

            bestScore = Math.Max(bestScore, eval);
            alpha = Math.Max(alpha, eval);

            // Prune the branch if alpha is greater than or equal to beta
            if (beta <= alpha)
            {
                break;
            }
        }

        // Store the result in the transposition table
        TranspositionTable[stateHash] = new TranspositionTableEntry
        {
            Score = bestScore,
            Depth = depth,
            Alpha = alpha,
            Beta = beta
        };

        return bestScore;
    }
}