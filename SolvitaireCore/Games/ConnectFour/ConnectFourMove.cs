namespace SolvitaireCore.ConnectFour;

public class ConnectFourMove(int column) : IMove<ConnectFourGameState>, IEquatable<ConnectFourMove>
{
    public bool IsTerminatingMove => false;
    public int Column { get; } = column;

    // Preallocate all possible moves (columns 0-6)
    public static readonly ConnectFourMove[] AllMoves = Enumerable.Range(0, ConnectFourGameState.Columns)
        .Select(col => new ConnectFourMove(col))
        .ToArray();
    
    public bool IsValid(ConnectFourGameState gameState)
    {
        // Top slot of the column must be empty
        return gameState.Board[0, Column] == 0;
    }

    public override string ToString() => $"Column {Column+1}";

    public bool Equals(ConnectFourMove? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Column == other.Column;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ConnectFourMove)obj);
    }

    public override int GetHashCode()
    {
        return Column;
    }
}