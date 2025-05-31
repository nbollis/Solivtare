namespace SolvitaireCore.TicTacToe;

public class TicTacToeMove : IMove<TicTacToeGameState>, IEquatable<TicTacToeMove>
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

    public bool Equals(TicTacToeMove? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Row == other.Row && Col == other.Col;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TicTacToeMove)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }
}