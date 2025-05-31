using NUnit.Framework;
using SolvitaireCore.Gomoku;

namespace Test.Games.Gomoku;

[TestFixture]
public class GomokuHeuristicEvaluatorTests
{
    private static GomokuHeuristicEvaluator ZeroEvaluator()
    {
        var eval = new GomokuHeuristicEvaluator
        {
            WeightTwoOpen = 0,
            WeightThreeOpen = 0,
            WeightFourOpen = 0,
            WeightFive = 0,
            WeightTwoGap = 0,
            WeightThreeGap = 0,
            WeightFourGap = 0,
            WeightTouchingOwn = 0,
            WeightTouchingOpponent = 0,
            WeightLiberty = 0,
        };
        return eval;
    }

    [Test]
    public void WinAndDraw_AreScoredCorrectly()
    {
        var state = new GomokuGameState(7);
        var eval = new GomokuHeuristicEvaluator();

        // Horizontal win for player 1
        for (int i = 0; i < 4; i++)
            state.Board[3,i] = 1; // Player 1
        state.ExecuteMove(new GomokuMove(3, 4)); // Player 1 wins
        Assert.That(eval.EvaluateState(state, 1), Is.EqualTo(eval.WeightFive));
        Assert.That(eval.EvaluateState(state, 2), Is.EqualTo(-eval.WeightFive));

        // Draw
        var drawState = new GomokuGameState(3);
        int player = 1;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                drawState.Board[r, c] = player;
                drawState.MovesMade++;
                player = 3 - player;
            }
        drawState.UndoMove(new GomokuMove(2, 2));
        drawState.ExecuteMove(new GomokuMove(2, 2));
        Assert.That(eval.EvaluateState(drawState, 1), Is.EqualTo(0));
    }

    [Test]
    public void OpenLines_AreCounted()
    {
        var state = new GomokuGameState(7);
        var eval = ZeroEvaluator();
        eval.WeightTwoOpen = 1;
        eval.WeightThreeOpen = 2;
        eval.WeightFourOpen = 3;

        // Place two in a row horizontally with open ends
        state.Board[3, 2] = 1;
        state.Board[3, 3] = 1;
        Assert.That(eval.EvaluateState(state, 1), Is.EqualTo(1)); // one open two

        // Place three in a row horizontally with open ends
        state.Board[3, 4] = 1;
        Assert.That(eval.EvaluateState(state, 1), Is.EqualTo(2)); // one open three

        // Place four in a row horizontally with open ends
        state.Board[3, 5] = 1;
        Assert.That(eval.EvaluateState(state, 1), Is.EqualTo(3)); // one open four
    }

    [Test]
    public void LinesWithGap_AreCounted()
    {
        var state = new GomokuGameState(7);
        var eval = ZeroEvaluator();
        eval.WeightTwoGap = 5;
        eval.WeightThreeGap = 7;
        eval.WeightFourGap = 9;

        // Two with a gap: X . X
        state.Board[3, 2] = 1;
        state.Board[3, 4] = 1;
        Assert.That(eval.EvaluateState(state, 1), Is.EqualTo(5));

        // Three with a gap: X . X X
        state.Board[3, 5] = 1;
        Assert.That(eval.EvaluateState(state, 1), Is.EqualTo(7)); // one three-gap, one two-gap

        // Four with a gap: X X . X X
        state.Board[3, 1] = 1;
        Assert.That(eval.EvaluateState(state, 1), Is.EqualTo(9)); // one four-gap, one three-gap, one two-gap
    }

    [Test]
    public void TouchingOwnAndOpponent_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4
        ---------
        . . . . .
        . . . . .
        . . X X .
        . . O . .
        . . . . .
        */
        var state = new GomokuGameState(5);
        var eval = ZeroEvaluator();
        eval.WeightTouchingOwn = 2;
        eval.WeightTouchingOpponent = 3;

        state.Board[2, 2] = 1; // X
        state.Board[2, 3] = 1; // X
        state.Board[3, 2] = 2; // O

        int playerOneEval = 2 * eval.WeightTouchingOwn + 2 * eval.WeightTouchingOpponent; // Two own, one opponent
        int playerTwoEval = 2 * eval.WeightTouchingOpponent; // two opponent

        Assert.That(eval.EvaluateState(state, 1), Is.EqualTo(playerOneEval - playerTwoEval));
    }
}
