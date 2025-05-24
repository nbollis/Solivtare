namespace SolvitaireCore.ConnectFour;
using SolvitaireCore;

public class ConnectFourAgent(StateEvaluator<ConnectFourGameState, ConnectFourMove> evaluator, int maxDepth = 4, string? name = null) : IAgent<ConnectFourGameState, ConnectFourMove>
{
    protected readonly StateEvaluator<ConnectFourGameState, ConnectFourMove> StateEvaluator = evaluator;
    public string Name => name ?? "Connect Four Agent";

    public ConnectFourMove GetNextAction(ConnectFourGameState state)
    {
        var legalMoves = state.GetLegalMoves();
        ConnectFourMove? bestMove = null;
        double bestScore = double.NegativeInfinity;

        foreach (var move in legalMoves)
        {
            state.ExecuteMove(move);
            double score = Minimax(state, maxDepth - 1, false, StateEvaluator);
            state.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove!;
    }

    private double Minimax(ConnectFourGameState state, int depth, bool maximizing, StateEvaluator<ConnectFourGameState, ConnectFourMove> heuristicEvaluator)
    {
        if (depth == 0 || state.IsGameWon || state.IsGameLost)
            return heuristicEvaluator.EvaluateState(state);

        var moves = state.GetLegalMoves();
        double best = maximizing ? double.NegativeInfinity : double.PositiveInfinity;

        foreach (var move in moves)
        {
            state.ExecuteMove(move);
            double score = Minimax(state, depth - 1, !maximizing, heuristicEvaluator);
            state.UndoMove(move);

            if (maximizing)
                best = Math.Max(best, score);
            else
                best = Math.Min(best, score);
        }

        return best;
    }
}