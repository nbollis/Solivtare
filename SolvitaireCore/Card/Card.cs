namespace SolvitaireCore;

/// <summary>
/// Represents a single card in a standard deck.
/// </summary>
/// <param name="suit"></param>
/// <param name="rank"></param>
public class Card(Suit suit, Rank rank, bool isFaceUp = false) : ICard
{
    public Suit Suit { get; } = suit;
    public Rank Rank { get; } = rank;
    public Color Color { get; } = suit.ToSuitColor();
    public bool IsFaceUp { get; set; } = isFaceUp;

    public override string ToString() => $"{Rank}{Suit.ToSuitCharacter()}";

    public bool Equals(ICard? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;
        return Suit == other.Suit && Rank == other.Rank;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Card)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Suit, (int)Rank);
    }
}