using SolvitaireCore;

namespace SolvitaireIO;

public class DeckDto
{
    public int Seed { get; set; }
    public int Shuffles { get; set; }
    public List<Card> Cards { get; set; }
}

public class MinimizedDeckDto() : IEquatable<MinimizedDeckDto>
{
    public int Seed { get; set; }
    public int Shuffles { get; set; }

    public MinimizedDeckDto(StandardDeck deck) : this()
    {
        Seed = deck.Seed;
        Shuffles = deck.Shuffles;
    }

    public MinimizedDeckDto(DeckDto dto) : this()
    {
        Seed = dto.Seed;
        Shuffles = dto.Shuffles;
    }

    public bool Equals(MinimizedDeckDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Seed == other.Seed && Shuffles == other.Shuffles;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((MinimizedDeckDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Seed, Shuffles);
    }
}