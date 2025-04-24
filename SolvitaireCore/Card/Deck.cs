using System.Collections;
using System.Text.Json;

namespace SolvitaireCore;

public abstract class Deck(int seed = 42) : Deck<Card>(seed) { }

public abstract class Deck<TCard> : ICloneable, IEnumerable<TCard>, IEquatable<Deck<TCard>> where TCard : ICard
{
    protected Random Random { get; }
    public List<TCard> Cards { get; private set; }

    protected Deck(int seed = 42)
    {
        Random = new Random(seed);
        Cards = new List<TCard>();
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
    }

    public object Clone()
    {
        var clonedDeck = (Deck<TCard>)Activator.CreateInstance(GetType())!;
        clonedDeck.Cards = [..Cards];
        return clonedDeck;
    }

    public TCard this[int index]
    {
        get => Cards[index];
        set => Cards[index] = value;
    }

    public static string SerializeDeck(Deck<Card> deck)
    {
        var serializableCards = deck.Cards.Select(c => new SerializableCard(c.Suit, c.Rank, c.IsFaceUp)).ToList();
        return JsonSerializer.Serialize(serializableCards, new JsonSerializerOptions { WriteIndented = true });
    }

    public static StandardDeck DeserializeDeck(string json)
    {
        var cards = JsonSerializer.Deserialize<List<SerializableCard>>(json)!;
        var deck = new StandardDeck(); // TODO: Make this work on non-standard decks. 
        deck.Cards.Clear(); // Clear default ordering

        foreach (var c in cards)
        {
            deck.Cards.Add(new Card(c.Suit, c.Rank, c.IsFaceUp));
        }
        return deck;
    }

    public IEnumerator<TCard> GetEnumerator() => Cards.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(Deck<TCard>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Cards.Count != other.Cards.Count) return false;
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
}