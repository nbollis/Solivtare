using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace Test.Games.ConnectFour;

[TestFixture]
public class ConnectFourMinimaxTests
{
    private ConnectFourHeuristicEvaluator _evaluator;
    private MinimaxAgent<ConnectFourGameState, ConnectFourMove> _agent;

    [SetUp]
    public void Setup()
    {
        _evaluator = new ConnectFourHeuristicEvaluator();
        _agent = new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(_evaluator, maxDepth: 4);
    }

    [Test]
    public void EvaluateState_IdentifiesWinLossDraw_Correctly()
    {
        /*
        Win for player 1:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        X X X X . . . 0
        */
        var stateWin = new ConnectFourGameState();
        for (int i = 0; i < 4; i++)
        {
            stateWin.ExecuteMove(new ConnectFourMove(i)); // P1
            if (i < 3) stateWin.ExecuteMove(new ConnectFourMove(6)); // P2
        }
        Assert.That(stateWin.IsPlayerWin(1), Is.True);
        Assert.That(_evaluator.EvaluateState(stateWin, 1), Is.EqualTo(1000));
        Assert.That(_evaluator.EvaluateState(stateWin, 2), Is.EqualTo(-1000));

        /*
        Win for player 2:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        O O O O . . . 0
        */
        var stateLoss = new ConnectFourGameState();
        for (int i = 0; i < 4; i++)
        {
            stateLoss.ExecuteMove(new ConnectFourMove(6)); // P1
            stateLoss.ExecuteMove(new ConnectFourMove(i)); // P2
        }
        Assert.That(stateLoss.IsPlayerWin(2), Is.True);
        Assert.That(_evaluator.EvaluateState(stateLoss, 2), Is.EqualTo(1000));
        Assert.That(_evaluator.EvaluateState(stateLoss, 1), Is.EqualTo(-1000));

        /*
        Draw:
        0 1 2 3 4 5 6
        -------------
        X O X O X O X 5
        O X O X O X O 4
        X O X O X O X 3
        O X O X O X O 2
        X O X O X O X 1
        O X O X O X O 0
        */
        var stateDraw = new ConnectFourGameState();
        var board = stateDraw.Board;
        for (int row = 0; row < ConnectFourGameState.Rows; row++)
        {
            for (int col = 0; col < ConnectFourGameState.Columns; col++)
            {
                int player = ((row + col) % 2) + 1;
                board[row, col] = player;
            }
        }
        stateDraw.SetBoard(board);
        stateDraw.SetCurrentPlayer(1);
        Assert.That(stateDraw.IsGameDraw, Is.True);
        Assert.That(_evaluator.EvaluateState(stateDraw, 1), Is.EqualTo(0));
        Assert.That(_evaluator.EvaluateState(stateDraw, 2), Is.EqualTo(0));
    }

    [Test]
    public void Minimax_AlternatesMaximizingAndMinimizing()
    {
        /*
        Player 1 can win in one move:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        X X X . . . . 0
        */
        var state = new ConnectFourGameState();
        for (int i = 0; i < 3; i++)
        {
            state.ExecuteMove(new ConnectFourMove(i)); // P1
            state.ExecuteMove(new ConnectFourMove(6)); // P2
        }
        var agent = new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(_evaluator, maxDepth: 2);
        var move = agent.GetNextAction(state);
        Assert.That(move.Column, Is.EqualTo(3), "Agent should take the winning move.");

        /*
        Player 2 can win if P1 doesn't block:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        O O O . . . . 0
        */
        state = new ConnectFourGameState();
        for (int i = 0; i < 3; i++)
        {
            state.ExecuteMove(new ConnectFourMove(6)); // P1
            state.ExecuteMove(new ConnectFourMove(i)); // P2
        }
        state.ExecuteMove(new ConnectFourMove(5)); // P1
        agent = new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(_evaluator, maxDepth: 2);
        move = agent.GetNextAction(state);
        Assert.That(move.Column, Is.EqualTo(3), "Agent should block the winning move for P2.");
    }

    [Test]
    public void Minimax_LeafEvaluation_IsFromMaximizingPlayerPerspective()
    {
        /*
        Symmetric board, no win/loss:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        . . . . . . . 0
        */
        var state = new ConnectFourGameState();
        state.SetCurrentPlayer(1);
        double eval1 = _evaluator.EvaluateState(state, 1);
        double eval2 = _evaluator.EvaluateState(state, 2);
        Assert.That(eval1, Is.EqualTo(-eval2).Within(1e-8), "Heuristic should be symmetric/opposite for each player.");

        /*
        Player 2 has won:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        . . . . . . . 1
        O O O O . . . 0
        */
        for (int i = 0; i < 4; i++)
        {
            state.ExecuteMove(new ConnectFourMove(6)); // P1
            state.ExecuteMove(new ConnectFourMove(i)); // P2
        }
        double eval = _evaluator.EvaluateState(state, 1);
        Assert.That(eval, Is.EqualTo(-1000), "Should be -1000 for maximizing player if opponent has won.");
    }

    [Test]
    public void Minimax_ComplexScenario_AvoidsImmediateLoss()
    {
        /*
        Player 1 to move, must block column 3 to avoid immediate loss:
        0 1 2 3 4 5 6
        -------------
        . . . . . . . 5
        . . . . . . . 4
        . . . . . . . 3
        . . . . . . . 2
        X . . . . . . 1
        X O O . O . X 0
        */
        var state = new ConnectFourGameState();
        // Bottom row
        state.ExecuteMove(new ConnectFourMove(0)); // P1
        state.ExecuteMove(new ConnectFourMove(1)); // P2
        state.ExecuteMove(new ConnectFourMove(0)); // P1
        state.ExecuteMove(new ConnectFourMove(2)); // P2
        state.ExecuteMove(new ConnectFourMove(6)); // P1
        state.ExecuteMove(new ConnectFourMove(4)); // P2

        // Now P1 must block column 3 or lose
        var agent = new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(_evaluator, maxDepth: 4);
        var move = agent.GetNextAction(state);
        Assert.That(move.Column, Is.EqualTo(3), "Agent should block the immediate loss in column 3.");
    }

    [Test]
    public void Minimax_ComplexScenario_FindsWinningFork()
    {
        /*  
        Player 1 to move, can create a fork for a double win:  
        0 1 2 3 4 5 6  
        -------------  
        . . . . . . r 5
        . . . . . . O 4
        . . . . . . X 3
        O X . . . . O 2
        X O X . . . O 1
        X O X f X . O 0 
        */
        var state = new ConnectFourGameState();

        // left side and one far right
        state.ExecuteMove(new ConnectFourMove(0)); // P1
        state.ExecuteMove(new ConnectFourMove(1)); // P2
        state.ExecuteMove(new ConnectFourMove(2)); // P1
        state.ExecuteMove(new ConnectFourMove(6)); // P2
        state.ExecuteMove(new ConnectFourMove(0)); // P1
        state.ExecuteMove(new ConnectFourMove(1)); // P2
        state.ExecuteMove(new ConnectFourMove(2)); // P1
        state.ExecuteMove(new ConnectFourMove(0)); // P2
        state.ExecuteMove(new ConnectFourMove(1)); // P1 

        // Right side - it big to ensure its P1's turn
        state.ExecuteMove(new ConnectFourMove(6)); // P2
        state.ExecuteMove(new ConnectFourMove(4)); // P1 
        state.ExecuteMove(new ConnectFourMove(6)); // P2
        state.ExecuteMove(new ConnectFourMove(6)); // P1
        state.ExecuteMove(new ConnectFourMove(6)); // P2


        var forkingMove = new ConnectFourMove(3); // P1 can play column 2 to create a fork
        var forkedState = (ConnectFourGameState)state.Clone();

        // Now P1 can play column 3 to create a fork  
        var agent = new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(_evaluator, maxDepth: 6);
        var move = agent.GetNextAction(state);
        Assert.That(move.Column, Is.EqualTo(3), "Agent should play the fork in column 3.");
        Assert.That(forkedState.IsGameWon, Is.False, "Forked state should not be a win before the winning move.");
        Assert.That(forkedState.IsGameDraw, Is.False, "Forked state should not be a draw before the winning move.");
        Assert.That(forkedState.CurrentPlayer, Is.EqualTo(1), "Forked state should still be P1's turn after evaluating the fork.");
        Assert.That(forkedState.MovesMade, Is.EqualTo(14), "Forked state should have the correct number of moves made before the winning move.");

        // Verify the forked state has two winning paths
        forkedState.ExecuteMove(forkingMove);
        forkedState.ExecuteMove(new(6));
        Assert.That(forkedState.IsGameWon, Is.False, "Forked state should not be a win yet.");

        var winningMoves = new List<ConnectFourMove>
        {
            new ConnectFourMove(0), // Win via column 0
            new ConnectFourMove(5)  // Win via column 5
        };
        foreach (var winningMove in winningMoves)
        {
            forkedState.ExecuteMove(winningMove);
            Assert.That(forkedState.IsPlayerWin(1), Is.True, "Forked state should allow a win with either move.");
            forkedState.UndoMove(winningMove);
        }

        // Ensure the agent can still find the winning move after the fork
        var winningMoveAfterFork = agent.GetNextAction(forkedState);
        Assert.That(winningMoveAfterFork.Column, Is.EqualTo(0).Or.EqualTo(5), "Agent should still find the winning move after the fork.");
        forkedState.ExecuteMove(winningMoveAfterFork);
        Assert.That(forkedState.IsPlayerWin(1), Is.True, "Forked state should be a win after the winning move.");
        Assert.That(forkedState.WinningPlayer, Is.EqualTo(1), "Winning player should be P1 after the winning move.");
        Assert.That(forkedState.WinningCells.Count, Is.GreaterThan(0), "Winning cells should be populated after the winning move.");
        Assert.That(forkedState.WinningCells.Count, Is.EqualTo(4), "Winning cells should contain 4 cells for the win.");
        Assert.That(forkedState.WinningCells, Does.Contain((5, 3)), "Winning cells should contain the winning move at (5, 3).");
        forkedState.UndoMove(winningMoveAfterFork);

        // Switch the current player, and verify the agent blocks the fork
        forkedState.UndoMove(forkingMove);
        forkedState.SetCurrentPlayer(2);
        var blockingMove = agent.GetNextAction(forkedState);
        Assert.That(blockingMove.Column, Is.EqualTo(3), "Agent should block the fork in column 3 when it's P2's turn.");
        forkedState.ExecuteMove(blockingMove);
        Assert.That(forkedState.IsGameWon, Is.False, "Blocking move should not result in a win for P2.");
        Assert.That(forkedState.IsGameDraw, Is.False, "Blocking move should not result in a draw.");
        Assert.That(forkedState.CurrentPlayer, Is.EqualTo(1), "Current player should switch back to P1 after the blocking move.");
    }
}
