namespace SolvitaireCore;

public class MinimaxAgent<TGameState, TMove> : BaseAgent<TGameState, TMove>, ISearchAgent<TGameState, TMove>
    where TGameState : ITwoPlayerGameState<TMove>
    where TMove : IMove
{
    protected readonly StateEvaluator<TGameState, TMove> Evaluator;
    protected readonly Random Rand;

    public int MaxDepth { get; set; }
    public MinimaxAgent(StateEvaluator<TGameState, TMove> evaluator, int maxDepth = 6)
    {
        Evaluator = evaluator;
        MaxDepth = maxDepth;
        Rand = new();
    }

    public override string Name => "Minimax Agent";

    public override TMove GetNextAction(TGameState gameState)
    {
        var legalMoves = gameState.GetLegalMoves();
        if (legalMoves.Count == 0)
            throw new InvalidOperationException("No legal moves available.");

        TMove bestMove = legalMoves[0]; 
        int maximizingPlayer = gameState.CurrentPlayer; // Capture the player of interest

        // Iterative deepening: search from depth 1 up to MaxDepth
        for (int depth = 1; depth <= MaxDepth; depth++)
        {
            double bestScore = double.NegativeInfinity;
            List<TMove> bestMoves = new();

            foreach (var move in legalMoves)
            {
                var clone = (TGameState)gameState.Clone();
                clone.ExecuteMove(move);

                // Assume the agent is always maximizing at the root
                double score = Minimax(clone, depth - 1, double.NegativeInfinity, double.PositiveInfinity, false, maximizingPlayer);

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

        return bestMove;
    }

    private double Minimax(TGameState state, int depth, double alpha, double beta, bool maximizingPlayer,
        int maximizingPlayerId)
    {
        int stateHash = state.GetHashCode();

        // Transposition table lookup
        if (TranspositionTable.TryGetValue(stateHash, out var entry))
        {
            if (entry.Depth >= depth && entry.Alpha <= alpha && entry.Beta >= beta)
                return entry.Score;
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
            return eval;
        }

        var moves = state.GetLegalMoves();
        if (moves.Count == 0)
        {
            double eval = Evaluator.EvaluateState(state, maximizingPlayerId);
            TranspositionTable[stateHash] = new TranspositionTableEntry
            {
                Score = eval,
                Depth = depth,
                Alpha = alpha,
                Beta = beta
            };
            return eval;
        }

        double bestScore = maximizingPlayer ? double.NegativeInfinity : double.PositiveInfinity;
        foreach (var move in moves)
        {
            var clone = (TGameState)state.Clone();
            clone.ExecuteMove(move);

            double score = Minimax(clone, depth - 1, alpha, beta, !maximizingPlayer, maximizingPlayerId);

            if (maximizingPlayer)
            {
                if (score > bestScore)
                    bestScore = score;
                if (bestScore > alpha)
                    alpha = bestScore;
            }
            else
            {
                if (score < bestScore)
                    bestScore = score;
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

        return bestScore;
    }
}
