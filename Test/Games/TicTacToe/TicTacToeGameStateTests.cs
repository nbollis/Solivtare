using SolvitaireCore;

namespace Test.Games.TicTacToe;

[TestFixture]
public class TicTacToeGameStateTests
{
    [Test]
    public void NewGame_HasEmptyBoard_AndPlayer1Starts()
    {
        var state = new TicTacToeGameState();
        Assert.That(state.CurrentPlayer, Is.EqualTo(1));
        Assert.That(state.MovesMade, Is.EqualTo(0));
        Assert.That(state.IsGameWon, Is.False);
        Assert.That(state.IsGameDraw, Is.False);

        for (int r = 0; r < TicTacToeGameState.Size; r++)
            for (int c = 0; c < TicTacToeGameState.Size; c++)
                Assert.That(state.Board[r, c], Is.EqualTo(0));
    }

    [Test]
    public void GetLegalMoves_AllCellsOpenAtStart()
    {
        var state = new TicTacToeGameState();
        var moves = state.GetLegalMoves();
        Assert.That(moves, Has.Count.EqualTo(TicTacToeGameState.Size * TicTacToeGameState.Size));
        for (int row = 0; row < TicTacToeGameState.Size; row++)
            for (int col = 0; col < TicTacToeGameState.Size; col++)
                Assert.That(moves.Exists(m => m.Row == row && m.Col == col), Is.True);
    }

    [Test]
    public void ExecuteMove_PlacesMarkAndSwitchesPlayer()
    {
        var state = new TicTacToeGameState();
        var move = new TicTacToeMove(1, 1);
        state.ExecuteMove(move);

        Assert.That(state.CurrentPlayer, Is.EqualTo(2));
        Assert.That(state.MovesMade, Is.EqualTo(1));
        Assert.That(state.Board[1, 1], Is.EqualTo(1));
    }

    [Test]
    public void UndoMove_RemovesMarkAndRestoresPlayer()
    {
        var state = new TicTacToeGameState();
        var move = new TicTacToeMove(0, 0);
        state.ExecuteMove(move);
        state.UndoMove(move);

        Assert.That(state.CurrentPlayer, Is.EqualTo(1));
        Assert.That(state.MovesMade, Is.EqualTo(0));
        Assert.That(state.Board[0, 0], Is.EqualTo(0));
    }

    [Test]
    public void WinDetection_Row()
    {
        var state = new TicTacToeGameState();
        for (int col = 0; col < 2; col++)
        {
            state.ExecuteMove(new TicTacToeMove(0, col)); // Player 1  
            state.ExecuteMove(new TicTacToeMove(1, col)); // Player 2  
        }
        state.ExecuteMove(new TicTacToeMove(0, 2)); // Player 1's 3rd in row 0  

        Assert.That(state.IsGameWon, Is.True);
        Assert.That(state.WinningPlayer, Is.EqualTo(1));
    }

    [Test]
    public void WinDetection_Column()
    {
        var state = new TicTacToeGameState();
        for (int row = 0; row < 2; row++)
        {
            state.ExecuteMove(new TicTacToeMove(row, 0)); // Player 1  
            state.ExecuteMove(new TicTacToeMove(row, 1)); // Player 2  
        }
        state.ExecuteMove(new TicTacToeMove(2, 0)); // Player 1's 3rd in column 0  

        Assert.That(state.IsGameWon, Is.True);
        Assert.That(state.WinningPlayer, Is.EqualTo(1));
    }

    [Test]
    public void WinDetection_MainDiagonal()
    {
        var state = new TicTacToeGameState();
        state.ExecuteMove(new TicTacToeMove(0, 0));
        state.ExecuteMove(new TicTacToeMove(1, 0));
        state.ExecuteMove(new TicTacToeMove(1, 1));
        state.ExecuteMove(new TicTacToeMove(2, 0));
        state.ExecuteMove(new TicTacToeMove(2, 2)); // Player 1's 3rd in main diagonal  

        Assert.That(state.IsGameWon, Is.True);
        Assert.That(state.WinningPlayer, Is.EqualTo(1));
    }

    [Test]
    public void WinDetection_AntiDiagonal()
    {
        var state = new TicTacToeGameState();

        state.ExecuteMove(new TicTacToeMove(0, 2));
        state.ExecuteMove(new TicTacToeMove(0, 1));
        state.ExecuteMove(new TicTacToeMove(1, 1));
        state.ExecuteMove(new TicTacToeMove(1, 0));
        state.ExecuteMove(new TicTacToeMove(2, 0)); // Player 1's 3rd in anti-diagonal 

        Assert.That(state.IsGameWon, Is.True);
        Assert.That(state.WinningPlayer, Is.EqualTo(1));
    }

    [Test]
    public void Clone_CreatesDeepCopy()
    {
        var state = new TicTacToeGameState();
        state.ExecuteMove(new TicTacToeMove(0, 0));
        var clone = (TicTacToeGameState)state.Clone();

        Assert.That(clone, Is.Not.SameAs(state));
        Assert.That(clone.CurrentPlayer, Is.EqualTo(state.CurrentPlayer));
        Assert.That(clone.MovesMade, Is.EqualTo(state.MovesMade));
        for (int r = 0; r < TicTacToeGameState.Size; r++)
            for (int c = 0; c < TicTacToeGameState.Size; c++)
                Assert.That(clone.Board[r, c], Is.EqualTo(state.Board[r, c]));
    }

    [Test]
    public void Equals_ReturnsTrueForIdenticalStates()
    {
        var state1 = new TicTacToeGameState();
        var state2 = new TicTacToeGameState();
        state1.ExecuteMove(new TicTacToeMove(0, 0));
        state2.ExecuteMove(new TicTacToeMove(0, 0));

        Assert.That(state1.Equals(state2), Is.True);
    }

    [Test]
    public void Equals_ReturnsFalseForDifferentStates()
    {
        var state1 = new TicTacToeGameState();
        var state2 = new TicTacToeGameState();
        state1.ExecuteMove(new TicTacToeMove(0, 0));
        state2.ExecuteMove(new TicTacToeMove(1, 1));

        Assert.That(state1.Equals(state2), Is.False);
    }

    [Test]
    public void IsGameDraw_WhenNoMovesLeftAndNoWin()
    {
        var state = new TicTacToeGameState();

        // Fill the board without a win  
        state.ExecuteMove(new TicTacToeMove(0, 2));
        state.ExecuteMove(new TicTacToeMove(0, 1));
        state.ExecuteMove(new TicTacToeMove(0, 0));
        state.ExecuteMove(new TicTacToeMove(1, 1));
        state.ExecuteMove(new TicTacToeMove(2, 1));
        state.ExecuteMove(new TicTacToeMove(1, 0));
        state.ExecuteMove(new TicTacToeMove(1, 2));
        state.ExecuteMove(new TicTacToeMove(2, 2));
        state.ExecuteMove(new TicTacToeMove(2, 0)); // Last move

        Assert.That(state.IsGameWon, Is.False);
        Assert.That(state.IsGameDraw, Is.True);
    }
}
