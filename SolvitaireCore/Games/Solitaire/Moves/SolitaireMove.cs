namespace SolvitaireCore;

public abstract class SolitaireMove(int fromPileIndex, int toPileIndex, bool shouldSkip = false)
    : IMove<SolitaireGameState>, IEquatable<SolitaireMove>
{
    public bool IsTerminatingMove { get; } = shouldSkip;
    public int FromPileIndex { get; } = fromPileIndex;
    public int ToPileIndex { get; } = toPileIndex;

    public abstract bool IsValid(SolitaireGameState gameState);

    public bool Equals(SolitaireMove? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return IsTerminatingMove == other.IsTerminatingMove && FromPileIndex == other.FromPileIndex && ToPileIndex == other.ToPileIndex;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SolitaireMove)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsTerminatingMove, FromPileIndex, ToPileIndex);
    }
}