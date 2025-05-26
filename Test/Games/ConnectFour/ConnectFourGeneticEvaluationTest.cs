using SolvitaireCore.ConnectFour;
using SolvitaireGenetics;

namespace Test.Games.ConnectFour;

[TestFixture]
public class ConnectFourGeneticEvaluationTest
{
    public static ConnectFourChromosome ZeroChromosome()
    {
        var chromosome = new ConnectFourChromosome();
        // Set all weights to 0
        foreach (var key in chromosome.MutableStatsByName.Keys.ToList())
            chromosome.MutableStatsByName[key] = 0.0;
        return chromosome;
    }

    [Test]
    public void PlayerTouchingCounts_AreCorrect()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        . . . X X . . 0
        . . . O . . . 1
        */
        var board = new int[6, 7];
        board[2, 3] = 1; // X
        board[2, 4] = 1; // X
        board[3, 3] = 2; // O

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.PlayerTouchingWeight, 1.0);
        chromosome.SetWeight(ConnectFourChromosome.OpponentTouchingWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // (2,3) touches (2,4) [own] and (3,3) [opponent] => 1 own, 1 opponent
        // (2,4) touches (2,3) [own] => 1 own
        // (3,3) touches (2,3) [opponent] => 1 opponent
        // So: playerTouching = 2, playerTouchingOpponent = 2, opponentTouching = 0, opponentTouchingPlayer = 2
        // Score = 1*2 + 1*1 - 1*0 - 1*1 = 2 + 1 - 0 - 1 = 2
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(2));
    }

    [Test]
    public void PlayerIsolatedCounts_AreCorrect()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        X . . . . . . 5
        . . . . . . . 4
        . . O . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        . . . . . . X 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; // X
        board[5, 6] = 1; // X
        board[2, 2] = 2; // O

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.IsolatedPieceWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // all pieces are isolated, so playerIsolated = 2, opponentIsolated = 1
        // Score = 1*2 - 1*1 = 1
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void PlayerSurroundedCounts_AreCorrect()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . O O O . . 2
        . . O X O . . 1
        . . O O O . . 0
        */
        var board = new int[6, 7];
        board[2, 2] = 1; // X
        board[1, 1] = 2; board[1, 2] = 2; board[1, 3] = 2;
        board[2, 1] = 2; board[2, 3] = 2;
        board[3, 1] = 2; board[3, 2] = 2; board[3, 3] = 2;

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.SurroundedPieceWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // playerSurrounded = 1, opponentSurrounded = 0
        // Score = 1*1 - 0*8 = 1
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void PlayerTwoInARowCounts_AreCorrect()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        O O . . . . . 1
        X X X . . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; board[0, 1] = 1; board[0, 2] = 1; // X X X
        board[1, 0] = 2; board[1, 1] = 2; // O O

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.TwoInARowWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // playerTwo = 1 (horizontal at top), opponentTwo = 2 (horizontal at row 1)
        // Score = 2*1 - 1*1 = 0
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void TwoInARowCounts_VerticalHorizontalDiagonal_AreCorrect()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . X . . . . 1
        X X X . . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; board[0, 1] = 1; board[0, 2] = 1; // X X X
        board[1, 2] = 1; // X

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.TwoInARowWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // playerTwo = 1 (horizontal at top), opponentTwo = 2 (horizontal at row 1)
        // Score = 2*1 - 1*1 = 0
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(4));
    }

    [Test]
    public void PlayerThreeInARowCounts_AreCorrect()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        X X X . . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; board[0, 1] = 1; board[0, 2] = 1; // X X X

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.ThreeInARowWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // playerThree = 1 (horizontal at top), opponentThree = 0
        // Score = 1*1 - 1*0 = 1
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void ThreeInARowCounts_VerticalHorizontalDiagonal_AreCorrect()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . X X . . . 2
        . . X . . . . 1
        X X X . . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; board[0, 1] = 1; board[0, 2] = 1; // X X X
        board[1, 2] = 1; board[2, 3] = 1; board[2, 2] = 1; // X X X

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.ThreeInARowWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(3));
    }

    [Test]
    public void PlayerTouchingOpponentCounts_AreCorrect()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        . . X O . . . 0
        */
        var board = new int[6, 7];
        board[0, 2] = 1; // X
        board[0, 3] = 2; // O

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.OpponentTouchingWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // (0,2) touches (0,3) [opponent], (0,3) touches (0,2) [opponent]
        // playerTouchingOpponent = 1, opponentTouchingPlayer = 1
        // Score = 1*1 - 1*1 = 0
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(0));
    }

    [Test]
    public void TwoInARow_HorizontalVerticalDiagonal_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . X . . . . . 1
        X X . . . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; // X
        board[0, 1] = 1; // X
        board[1, 1] = 1; // X

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.TwoInARowWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // Only one horizontal two-in-a-row for player 1
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(3));
    }

    [Test]
    public void TwoWithOneGap_Horizontal_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        X . X . . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; // X
        board[0, 2] = 1; // X

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.TwoWithOneGapWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // One two-with-gap for player 1 (positions 0,1,2,3: X . X .)
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void ThreeInARow_HorizontalAndVertical_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        X X X . . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; // X
        board[0, 1] = 1; // X
        board[0, 2] = 1; // X

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.ThreeInARowWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // Only one horizontal three-in-a-row for player 1
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void ThreeWithOneGap_Horizontal_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        X . X X . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; // X
        board[0, 2] = 1; // X
        board[0, 3] = 1; // X

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.ThreeWithOneGapWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // One three-with-gap for player 1 (positions 0,1,2,3: X . X X)
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void TwoWithOneGap_Vertical_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        X . . . . . . 2
        . . . . . . . 1
        X . . . . . . 0
        */
        var board = new int[6, 7];
        board[0, 0] = 1; // X
        board[2, 0] = 1; // X

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.TwoWithOneGapWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // One vertical two-with-gap for player 1 (positions 0,1,2,3: X . X .)
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void TwoWithOneGap_DiagonalVerticalHorizontal_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . X . . . . 3
        . . . . . . . 2
        X . X . . . . 1
        . . . . . . . 0
        */
        var board = new int[6, 7];
        board[1, 0] = 1; // X
        board[3, 2] = 1; // X
        board[1, 2] = 1; // X (gap in the diagonal)

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.TwoWithOneGapWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // One diagonal three-with-gap for player 1
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(3));
    }

    [Test]
    public void ThreeWithOneGap_Diagonal_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . X . . . 4
        . . . . . . . 3
        . X . . . . . 2
        X . X . . . . 1
        . . . . . . . 0
        */
        var board = new int[6, 7];
        board[1, 0] = 1; // X
        board[2, 1] = 1; // X
        board[4, 3] = 1; // X
        board[1, 2] = 1; // X (gap in the diagonal)

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.ThreeWithOneGapWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // One diagonal three-with-gap for player 1
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(1));
    }

    [Test]
    public void ThreeWithOneGap_DiagonalHorizontalDiagonal_AreCounted()
    {
        /*
        Board:
        0 1 2 3 4 5 6
        -------------
        . X . . . . . 5
        . . . X . . . 4
        . X . . . . . 3
        . X . . . . . 2
        X . X X . . . 1
        . . . . . . . 0
        */
        var board = new int[6, 7];
        board[1, 0] = 1; // X
        board[2, 1] = 1; // X
        board[3, 1] = 1; // X
        board[5, 1] = 1; // X
        board[4, 3] = 1; // X
        board[1, 3] = 1; // X
        board[1, 2] = 1; // X (gap in the diagonal)

        var state = new ConnectFourGameState();
        state.SetBoard(board);
        state.SetCurrentPlayer(1);

        var chromosome = ZeroChromosome();
        chromosome.SetWeight(ConnectFourChromosome.ThreeWithOneGapWeight, 1.0);

        var evaluator = new GeneticConnectFourEvaluator(chromosome);

        // One diagonal three-with-gap for player 1
        Assert.That(evaluator.EvaluateState(state, 1), Is.EqualTo(3));
    }
}