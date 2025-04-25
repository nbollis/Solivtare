namespace SolvitaireCore;

/// <summary>  
/// A simple evaluation agent that uses a heuristic evaluation function to select the best move.  
/// </summary>  
public class AlphaBetaEvaluationAgent(SolitaireEvaluator evaluator, int maxLookahead = 10) : SolitaireAgent
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
                double score = EvaluateWithLookahead(gameState, depth - 1, alpha, beta, false);
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

    private double EvaluateWithLookahead(SolitaireGameState gameState, int depth, double alpha, double beta, bool isMaximizingPlayer)
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

        double bestScore;
        if (isMaximizingPlayer)
        {
            bestScore = double.NegativeInfinity;

            foreach (var move in gameState.GetLegalMoves())
            {
                gameState.ExecuteMove(move);
                double eval = EvaluateWithLookahead(gameState, depth - 1, alpha, beta, false);
                gameState.UndoMove(move);

                bestScore = Math.Max(bestScore, eval);
                alpha = Math.Max(alpha, eval);

                // Prune the branch if alpha is greater than or equal to beta
                if (beta <= alpha)
                {
                    break;
                }
            }
        }
        else
        {
            bestScore = double.PositiveInfinity;

            foreach (var move in gameState.GetLegalMoves())
            {
                gameState.ExecuteMove(move);
                double eval = EvaluateWithLookahead(gameState, depth - 1, alpha, beta, true);
                gameState.UndoMove(move);

                bestScore = Math.Min(bestScore, eval);
                beta = Math.Min(beta, eval);

                // Prune the branch if alpha is greater than or equal to beta
                if (beta <= alpha)
                {
                    break;
                }
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