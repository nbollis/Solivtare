using SolvitaireCore.ConnectFour;

namespace Test.Games.ConnectFour;

[TestFixture]
public class ConnectFourGameStateTests
{
    [Test]
    public void NewGame_HasEmptyBoard_AndPlayer1Starts()
    {
        var state = new ConnectFourGameState();
        Assert.That(state.CurrentPlayer, Is.EqualTo(1));
        Assert.That(state.MovesMade, Is.EqualTo(0));
        Assert.That(state.IsGameWon, Is.False);
        Assert.That(state.IsGameLost, Is.False);

        for (int r = 0; r < ConnectFourGameState.Rows; r++)
            for (int c = 0; c < ConnectFourGameState.Columns; c++)
                Assert.That(state.Board[r, c], Is.EqualTo(0));
    }

    [Test]
    public void GetLegalMoves_AllColumnsOpenAtStart()
    {
        var state = new ConnectFourGameState();
        var moves = state.GetLegalMoves();
        Assert.That(moves, Has.Count.EqualTo(ConnectFourGameState.Columns));
        for (int col = 0; col < ConnectFourGameState.Columns; col++)
            Assert.That(moves.Exists(m => m.Column == col), Is.True);
    }

    [Test]
    public void ExecuteMove_PlacesDiscAndSwitchesPlayer()
    {
        var state = new ConnectFourGameState();
        var move = new ConnectFourMove(3);
        state.ExecuteMove(move);

        Assert.That(state.CurrentPlayer, Is.EqualTo(2));
        Assert.That(state.MovesMade, Is.EqualTo(1));
        Assert.That(state.Board[ConnectFourGameState.Rows - 1, 3], Is.EqualTo(1));
    }

    [Test]
    public void UndoMove_RemovesDiscAndRestoresPlayer()
    {
        var state = new ConnectFourGameState();
        var move = new ConnectFourMove(2);
        state.ExecuteMove(move);
        state.UndoMove(move);

        Assert.That(state.CurrentPlayer, Is.EqualTo(1));
        Assert.That(state.MovesMade, Is.EqualTo(0));
        Assert.That(state.Board[ConnectFourGameState.Rows - 1, 2], Is.EqualTo(0));
    }

    [Test]
    public void WinDetection_Vertical()
    {
        var state = new ConnectFourGameState();
        for (int i = 0; i < 3; i++)
        {
            state.ExecuteMove(new ConnectFourMove(0)); // Player 1
            state.ExecuteMove(new ConnectFourMove(1)); // Player 2
        }
        state.ExecuteMove(new ConnectFourMove(0)); // Player 1's 4th in column 0

        Assert.That(state.IsGameWon, Is.True);
    }

    [Test]
    public void WinDetection_Horizontal()
    {
        var state = new ConnectFourGameState();
        for (int col = 0; col < 3; col++)
        {
            state.ExecuteMove(new ConnectFourMove(col)); // Player 1
            state.ExecuteMove(new ConnectFourMove(col)); // Player 2
        }
        state.ExecuteMove(new ConnectFourMove(3)); // Player 1's 4th in a row

        Assert.That(state.IsGameWon, Is.True);
    }

    [Test]
    public void Clone_CreatesDeepCopy()
    {
        var state = new ConnectFourGameState();
        state.ExecuteMove(new ConnectFourMove(0));
        var clone = (ConnectFourGameState)state.Clone();

        Assert.That(clone, Is.Not.SameAs(state));
        Assert.That(clone.CurrentPlayer, Is.EqualTo(state.CurrentPlayer));
        Assert.That(clone.MovesMade, Is.EqualTo(state.MovesMade));
        for (int r = 0; r < ConnectFourGameState.Rows; r++)
            for (int c = 0; c < ConnectFourGameState.Columns; c++)
                Assert.That(clone.Board[r, c], Is.EqualTo(state.Board[r, c]));
    }

    [Test]
    public void Equals_ReturnsTrueForIdenticalStates()
    {
        var state1 = new ConnectFourGameState();
        var state2 = new ConnectFourGameState();
        state1.ExecuteMove(new ConnectFourMove(0));
        state2.ExecuteMove(new ConnectFourMove(0));

        Assert.That(state1.Equals(state2), Is.True);
    }

    [Test]
    public void Equals_ReturnsFalseForDifferentStates()
    {
        var state1 = new ConnectFourGameState();
        var state2 = new ConnectFourGameState();
        state1.ExecuteMove(new ConnectFourMove(0));
        state2.ExecuteMove(new ConnectFourMove(1));

        Assert.That(state1.Equals(state2), Is.False);
    }

    [Test]
    public void IsGameDraw_WhenNoMovesLeftAndNoWin()
    {
        var state = new ConnectFourGameState();
        // Fill the board without a win
        for (int col = 0; col < ConnectFourGameState.Columns; col++)
            for (int row = 0; row < ConnectFourGameState.Rows; row++)
                state.Board[row, col] = (row + col) % 2 + 1;
        Assert.That(state.IsGameWon, Is.False);
        Assert.That(state.IsGameDraw, Is.True);
    }
}