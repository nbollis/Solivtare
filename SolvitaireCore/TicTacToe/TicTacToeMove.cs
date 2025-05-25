namespace SolvitaireCore;

public class TicTacToeMove : IMove<TicTacToeGameState>
{
    public int Row { get; }
    public int Col { get; }
    public bool IsTerminatingMove => false;

    public TicTacToeMove(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public bool IsValid(TicTacToeGameState gameState)
        => gameState.Board[Row, Col] == 0;

    public override string ToString() => $"({Row},{Col})";
}