using SolvitaireCore;
using SolvitaireCore.Gomoku;

namespace Test.Games.Gomoku;

[TestFixture]
public class GomokuMinimaxTests
{
    private GomokuHeuristicEvaluator _evaluator;
    private MinimaxAgent<GomokuGameState, GomokuMove> _agent;

    [SetUp]
    public void Setup()
    {
        _evaluator = new GomokuHeuristicEvaluator();
        _agent = new MinimaxAgent<GomokuGameState, GomokuMove>(_evaluator, maxDepth: 3);
    }

    [Test]
    public void EvaluateState_IdentifiesWinLossDraw_Correctly()
    {
        // Win for player 1 (horizontal)
        var stateWin = new GomokuGameState(7);
        for (int i = 0; i < 5; i++)
        {
            stateWin.ExecuteMove(new GomokuMove(3, i)); // P1
            if (i < 4) stateWin.ExecuteMove(new GomokuMove(4, i)); // P2
        }
        Assert.That(stateWin.IsPlayerWin(1), Is.True);
        Assert.That(_evaluator.EvaluateState(stateWin, 1), Is.EqualTo(_evaluator.WeightFive));
        Assert.That(_evaluator.EvaluateState(stateWin, 2), Is.EqualTo(-_evaluator.WeightFive));

        // Win for player 2 (vertical)
        var stateLoss = new GomokuGameState(7);
        for (int i = 0; i < 5; i++)
        {
            stateLoss.ExecuteMove(new GomokuMove(i, 0)); // P1
            stateLoss.ExecuteMove(new GomokuMove(i, 1)); // P2
        }
        Assert.That(stateLoss.IsPlayerWin(1), Is.True);
        Assert.That(_evaluator.EvaluateState(stateLoss, 1), Is.EqualTo(_evaluator.WeightFive));
        Assert.That(_evaluator.EvaluateState(stateLoss, 2), Is.EqualTo(-_evaluator.WeightFive));

        // Draw
        var stateDraw = new GomokuGameState(3);
        int player = 1;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                stateDraw.Board[r, c] = player;
                stateDraw.MovesMade++;
                player = 3 - player;
            }
        stateDraw.UndoMove(new GomokuMove(2, 2));
        stateDraw.ExecuteMove(new GomokuMove(2, 2));
        Assert.That(stateDraw.IsGameDraw, Is.True);
        Assert.That(_evaluator.EvaluateState(stateDraw, 1), Is.EqualTo(0));
        Assert.That(_evaluator.EvaluateState(stateDraw, 2), Is.EqualTo(0));
    }

    [Test]
    public void Minimax_ImmediateWinAndBlock()
    {
        // Player 1 can win in one move (horizontal)
        var state = new GomokuGameState(7);
        for (int i = 0; i < 4; i++)
        {
            state.ExecuteMove(new GomokuMove(3, i)); // P1
            state.ExecuteMove(new GomokuMove(4, i)); // P2
        }
        var move = _agent.GetNextAction(state);
        Assert.That(move.Row, Is.EqualTo(3));
        Assert.That(move.Col, Is.EqualTo(4), "Agent should take the winning move.");

        // Player 2 can win if P1 doesn't block (vertical)
        state = new GomokuGameState(7);
        for (int i = 0; i < 4; i++)
        {
            state.ExecuteMove(new GomokuMove(i, 0)); // P1
            state.ExecuteMove(new GomokuMove(i, 1)); // P2
        }
        state.ExecuteMove(new GomokuMove(6, 6)); // P1 random move
        _agent = new MinimaxAgent<GomokuGameState, GomokuMove>(_evaluator, maxDepth: 2);
        move = _agent.GetNextAction(state);
        Assert.That(move.Row, Is.EqualTo(4));
        Assert.That(move.Col, Is.EqualTo(1), "Agent should block the winning move for P2.");
    }

    [Test]
    public void Minimax_LeafEvaluation_IsFromMaximizingPlayerPerspective()
    {
        // Symmetric board, no win/loss
        var state = new GomokuGameState(7);
        state.SetCurrentPlayer(1);
        double eval1 = _evaluator.EvaluateState(state, 1);
        double eval2 = _evaluator.EvaluateState(state, 2);
        Assert.That(eval1, Is.EqualTo(-eval2).Within(1e-8), "Heuristic should be symmetric/opposite for each player.");

        // Player 1 has won
        for (int i = 0; i < 5; i++)
        {
            state.ExecuteMove(new GomokuMove(i, 0)); // P1
            state.ExecuteMove(new GomokuMove(i, 1)); // P2
        }
        double eval = _evaluator.EvaluateState(state, 1);
        Assert.That(eval, Is.EqualTo(_evaluator.WeightFive), "Should be WeightFive for maximizing player if opponent has won.");

        double lostEval = _evaluator.EvaluateState(state, 2);
        Assert.That(lostEval, Is.EqualTo(-_evaluator.WeightFive), "Should be -WeightFive for minimizing player if opponent has won.");

    }
}
