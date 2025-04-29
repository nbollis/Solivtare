using System.Text.Json;
using System.Text.Json.Serialization;
using SolvitaireCore;

namespace SolvitaireIO;

public static class DeckSerializer
{
    private static readonly JsonSerializerOptions VerboseOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never, // Ensure all properties are serialized
        WriteIndented = true,
    };
    private static readonly JsonSerializerOptions MinimalisticOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never, // Ensure all properties are serialized
        WriteIndented = true,
    };

    #region Verbose Card Serialization

    /// <summary>
    /// Serializes a deck into a JSON string.
    /// </summary>
    public static string SerializeDeck<TCard>(Deck<TCard> deck) where TCard : class, ICard
    {
        var dto = new DeckDto
        {
            Seed = deck.Seed,
            Shuffles = deck.Shuffles,
            Cards = deck.Cards.Cast<Card>().ToList()
        };

        return JsonSerializer.Serialize(dto, VerboseOptions);
    }

    public static string SerializeStandardDecks(List<StandardDeck> decks)
    {
        var dtos = decks.Select(deck => new DeckDto
        {
            Seed = deck.Seed,
            Shuffles = deck.Shuffles,
            Cards = deck.Cards
        }).ToList();
        return JsonSerializer.Serialize(dtos, VerboseOptions);
    }

    /// <summary>
    /// Deserializes a JSON string into a StandardDeck.
    /// </summary>
    public static StandardDeck DeserializeStandardDeck(string json)
    {
        var dto = JsonSerializer.Deserialize<DeckDto>(json, VerboseOptions)!;

        var deck = new StandardDeck(dto.Seed)
        {
            Shuffles = dto.Shuffles
        };

        deck.Cards.Clear();
        foreach (var card in dto.Cards)
        {
            deck.Cards.Add(new Card(card.Suit, card.Rank, card.IsFaceUp));
        }

        return deck;
    }

    /// <summary>
    /// Deserializes a JSON string into a list of StandardDecks.
    /// </summary>
    public static List<StandardDeck> DeserializeStandardDecks(string json)
    {
        List<DeckDto> dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<DeckDto>>(json)!;
        }
        catch (JsonException) // Deck is in my goofy format.  
        {
            // Sanitize input by adding a comma after each closing square bracket except the last  
            int lastBracketIndex = json.LastIndexOf("}{");
            if (lastBracketIndex > 0)
            {
                json = "[" + json.Substring(0, lastBracketIndex).Replace("}{", "},{") + json.Substring(lastBracketIndex) + "]";
            }
            dtos = JsonSerializer.Deserialize<List<DeckDto>>(json)!;
        }

        var decks = new List<StandardDeck>();
        foreach (var dto in dtos)
        {
            var deck = new StandardDeck(dto.Seed)
            {
                Shuffles = dto.Shuffles
            };

            deck.Cards.Clear();
            foreach (var card in dto.Cards)
            {
                deck.Cards.Add(new Card(card.Suit, card.Rank, card.IsFaceUp));
            }

            decks.Add(deck);
        }

        return decks;
    }

    #endregion

    #region Minimalistic Card Serialization

    public static string SerializeMinimalDeck(StandardDeck deck)
    {
        var dto = new MinimizedDeckDto()
        {
            Seed = deck.Seed,
            Shuffles = deck.Shuffles
        };
        return JsonSerializer.Serialize(dto, MinimalisticOptions);
    }

    public static string SerializeMinimalDecks(List<StandardDeck> decks)
    {
        var dtos = decks.Select(deck => new MinimizedDeckDto
        {
            Seed = deck.Seed,
            Shuffles = deck.Shuffles
        }).ToList();
        return JsonSerializer.Serialize(dtos, MinimalisticOptions);
    }

    public static StandardDeck DeserializeMinimalDeck(string json)
    {
        var dto = JsonSerializer.Deserialize<MinimizedDeckDto>(json, MinimalisticOptions)!;

        var deck = new StandardDeck(dto.Seed);
        for (int i = 0; i < dto.Shuffles; i++)
        {
            deck.Shuffle();
        }

        return deck;
    }

    public static List<StandardDeck> DeserializeMinimalDecks(string json)
    {
        var dtos = JsonSerializer.Deserialize<List<MinimizedDeckDto>>(json, MinimalisticOptions)!;
        var decks = new List<StandardDeck>();
        foreach (var dto in dtos)
        {
            var deck = new StandardDeck(dto.Seed);
            for (int i = 0; i < dto.Shuffles; i++)
            {
                deck.Shuffle();
            }
            decks.Add(deck);
        }
        return decks;
    }

    #endregion
}