using NUnit.Framework;
using SolvitaireCore.Gomoku;

namespace Test.Games.Gomoku;

[TestFixture]
public class GomokuGameStateTests
{
    [Test]
    public void NewGame_HasEmptyBoard_AndPlayer1Starts()
    {
        var state = new GomokuGameState();
        Assert.That(state.CurrentPlayer, Is.EqualTo(1));
        Assert.That(state.MovesMade, Is.EqualTo(0));
        Assert.That(state.IsGameWon, Is.False);
        Assert.That(state.IsGameDraw, Is.False);

        for (int r = 0; r < state.BoardSize; r++)
            for (int c = 0; c < state.BoardSize; c++)
                Assert.That(state.Board[r, c], Is.EqualTo(0));
    }

    [Test]
    public void GetLegalMoves_AllCellsOpenAtStart()
    {
        var state = new GomokuGameState();
        var moves = state.GetLegalMoves();
        Assert.That(moves, Has.Count.EqualTo(state.BoardSize * state.BoardSize));
        for (int row = 0; row < state.BoardSize; row++)
            for (int col = 0; col < state.BoardSize; col++)
                Assert.That(moves.Exists(m => m.Row == row && m.Col == col), Is.True);
    }

    [Test]
    public void ExecuteMove_PlacesStoneAndSwitchesPlayer()
    {
        var state = new GomokuGameState();
        var move = new GomokuMove(7, 7);
        state.ExecuteMove(move);

        Assert.That(state.CurrentPlayer, Is.EqualTo(2));
        Assert.That(state.MovesMade, Is.EqualTo(1));
        Assert.That(state.Board[7, 7], Is.EqualTo(1));
    }

    [Test]
    public void UndoMove_RemovesStoneAndRestoresPlayer()
    {
        var state = new GomokuGameState();
        var move = new GomokuMove(0, 0);
        state.ExecuteMove(move);
        state.UndoMove(move);

        Assert.That(state.CurrentPlayer, Is.EqualTo(1));
        Assert.That(state.MovesMade, Is.EqualTo(0));
        Assert.That(state.Board[0, 0], Is.EqualTo(0));
    }

    [Test]
    public void WinDetection_Horizontal()
    {
        var state = new GomokuGameState();
        int row = 7;
        for (int col = 0; col < 4; col++)
        {
            state.ExecuteMove(new GomokuMove(row, col)); // Player 1
            state.ExecuteMove(new GomokuMove(row + 1, col)); // Player 2
        }
        state.ExecuteMove(new GomokuMove(row, 4)); // Player 1's 5th in a row

        Assert.That(state.IsGameWon, Is.True);
        Assert.That(state.WinningPlayer, Is.EqualTo(1));
        Assert.That(state.WinningCells, Has.Count.EqualTo(5));
    }

    [Test]
    public void WinDetection_Vertical()
    {
        var state = new GomokuGameState();
        int col = 7;
        for (int row = 0; row < 4; row++)
        {
            state.ExecuteMove(new GomokuMove(row, col)); // Player 1
            state.ExecuteMove(new GomokuMove(row, col + 1)); // Player 2
        }
        state.ExecuteMove(new GomokuMove(4, col)); // Player 1's 5th in a column

        Assert.That(state.IsGameWon, Is.True);
        Assert.That(state.WinningPlayer, Is.EqualTo(1));
        Assert.That(state.WinningCells, Has.Count.EqualTo(5));
    }

    [Test]
    public void WinDetection_MainDiagonal()
    {
        var state = new GomokuGameState();
        for (int i = 0; i < 4; i++)
        {
            state.ExecuteMove(new GomokuMove(i, i)); // Player 1
            state.ExecuteMove(new GomokuMove(i, i + 1)); // Player 2
        }
        state.ExecuteMove(new GomokuMove(4, 4)); // Player 1's 5th in main diagonal

        Assert.That(state.IsGameWon, Is.True);
        Assert.That(state.WinningPlayer, Is.EqualTo(1));
        Assert.That(state.WinningCells, Has.Count.EqualTo(5));
    }

    [Test]
    public void WinDetection_AntiDiagonal()
    {
        var state = new GomokuGameState();
        int size = state.BoardSize;
        for (int i = 0; i < 4; i++)
        {
            state.ExecuteMove(new GomokuMove(i, size - 1 - i)); // Player 1
            state.ExecuteMove(new GomokuMove(i, size - 2 - i)); // Player 2
        }
        state.ExecuteMove(new GomokuMove(4, size - 1 - 4)); // Player 1's 5th in anti-diagonal

        Assert.That(state.IsGameWon, Is.True);
        Assert.That(state.WinningPlayer, Is.EqualTo(1));
        Assert.That(state.WinningCells, Has.Count.EqualTo(5));
    }

    [Test]
    public void Clone_CreatesDeepCopy()
    {
        var state = new GomokuGameState();
        state.ExecuteMove(new GomokuMove(0, 0));
        var clone = (GomokuGameState)state.Clone();

        Assert.That(clone, Is.Not.SameAs(state));
        Assert.That(clone.CurrentPlayer, Is.EqualTo(state.CurrentPlayer));
        Assert.That(clone.MovesMade, Is.EqualTo(state.MovesMade));
        for (int r = 0; r < state.BoardSize; r++)
            for (int c = 0; c < state.BoardSize; c++)
                Assert.That(clone.Board[r, c], Is.EqualTo(state.Board[r, c]));
    }

    [Test]
    public void Equals_ReturnsTrueForIdenticalStates()
    {
        var state1 = new GomokuGameState();
        var state2 = new GomokuGameState();
        state1.ExecuteMove(new GomokuMove(0, 0));
        state2.ExecuteMove(new GomokuMove(0, 0));

        Assert.That(state1.Equals(state2), Is.True);
    }

    [Test]
    public void Equals_ReturnsFalseForDifferentStates()
    {
        var state1 = new GomokuGameState();
        var state2 = new GomokuGameState();
        state1.ExecuteMove(new GomokuMove(0, 0));
        state2.ExecuteMove(new GomokuMove(1, 1));

        Assert.That(state1.Equals(state2), Is.False);
    }

    [Test]
    public void IsGameDraw_WhenNoMovesLeftAndNoWin()
    {
        var state = new GomokuGameState(5);
        // Fill the board with alternating players, no five in a row
        int player = 1;
        for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
            {
                state.Board[r, c] = player;
                state.MovesMade++;
                player = 3 - player;
            }
        // Force update of draw state
        state.UndoMove(new GomokuMove(4, 4));
        state.ExecuteMove(new GomokuMove(4, 4));

        Assert.That(state.IsGameWon, Is.False);
        Assert.That(state.IsGameDraw, Is.True);
    }
}
