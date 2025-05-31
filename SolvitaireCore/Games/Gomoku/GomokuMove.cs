namespace SolvitaireCore.Gomoku;

public sealed record GomokuMove(int Row, int Col) : IMove<GomokuGameState>, IEquatable<GomokuMove>
{
    public bool IsTerminatingMove => false; 
    public bool IsValid(GomokuGameState gameState)
        => gameState.Board[Row, Col] == 0;
    public bool Equals(GomokuMove? other) => other is not null && Row == other.Row && Col == other.Col;
    public override int GetHashCode() => HashCode.Combine(Row, Col);
    public override string ToString() => $"({Row}, {Col})";
}