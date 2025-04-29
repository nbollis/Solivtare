using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SolvitaireCore;

public abstract class Deck(int seed = 42) : Deck<Card>(seed) { }

public abstract class Deck<TCard> : ICloneable, IEnumerable<TCard>, IEquatable<Deck<TCard>> where TCard : class, ICard
{
    protected Random Random { get; }

    [JsonPropertyName("seed")]
    public int Seed { get; set; }

    [JsonPropertyName("shuffles")]
    public int Shuffles { get; set; }

    [JsonPropertyName("cards")]
    public List<TCard> Cards { get; private set; }

    protected Deck(int seed = 42)
    {
        Random = new Random(seed);
        Cards = new List<TCard>();
        Seed = seed;
        Shuffles = 0;
    }

    public TCard DrawCard(int toDraw = 1)
    {
        if (Cards.Count == 0)
            throw new InvalidOperationException("No cards left in the deck.");

        if (toDraw > Cards.Count)
            toDraw = Cards.Count;

        var drawnCards = Cards.Take(toDraw).ToList();
        Cards.RemoveRange(0, toDraw);
        return drawnCards.First();
    }

    public void Shuffle()
    {
        int n = Cards.Count;
        while (n > 1)
        {
            int k = Random.Next(n--);
            (Cards[k], Cards[n]) = (Cards[n], Cards[k]);
        }

        Shuffles++;
    }

    public object Clone()
    {
        var clonedDeck = (Deck<TCard>)Activator.CreateInstance(GetType(), Seed)!;
        clonedDeck.Cards = [..Cards];
        clonedDeck.Shuffles = Shuffles;
        return clonedDeck;
    }

    public TCard this[int index]
    {
        get => Cards[index];
        set => Cards[index] = value;
    }

    public void FlipAllCardsDown()
    {
        foreach (var card in Cards)
        {
            card.IsFaceUp = false;
        }
    }

    public bool Equals(Deck<TCard>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Cards.Count != other.Cards.Count) return false;
        if (Seed != other.Seed) return false;
        if (Shuffles != other.Shuffles) return false;
        for (int i = 0; i < Cards.Count; i++)
        {
            ICard thisCard = Cards[i];
            ICard otherCard = other.Cards[i];
            if (!thisCard.Equals(otherCard))
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Deck<TCard>)obj);
    }

    public override int GetHashCode()
    {
        return Cards.GetHashCode();
    }

    public IEnumerator<TCard> GetEnumerator() => Cards.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

