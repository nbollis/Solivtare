using System.Text.Json;

namespace SolvitaireCore;

public class StandardDeck : Deck
{
    public StandardDeck() : this(42) { }
    public StandardDeck(int seed = 42) : base(seed)
    {
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                Cards.Add(new Card(suit, rank));
    }

    public static StandardDeck DeserializeDeck(string json)
    {
        var cards = JsonSerializer.Deserialize<List<Card>>(json)!;
        var deck = new StandardDeck();
        deck.Cards.Clear();

        foreach (var c in cards)
        {
            deck.Cards.Add(new(c.Suit, c.Rank, c.IsFaceUp));
        }
        return deck;
    }

    public static List<StandardDeck> DeserializeDecks(string json)
    {
        List<List<Card>> decks;
        try
        {
            decks = JsonSerializer.Deserialize<List<List<Card>>>(json)!;
        }
        catch (JsonException) // Deck is in my goofy format.  
        {
            // Sanitize input by adding a comma after each closing square bracket except the last  
            int lastBracketIndex = json.LastIndexOf(']');
            if (lastBracketIndex > 0)
            {
                json = "[" + json.Substring(0, lastBracketIndex).Replace("]", "],") + json.Substring(lastBracketIndex) + "]";
            }
            decks = JsonSerializer.Deserialize<List<List<Card>>>(json)!;
        }

        var standardDecks = new List<StandardDeck>();
        foreach (var cardList in decks)
        {
            var deck = new StandardDeck();
            deck.Cards.Clear();

            foreach (var card in cardList)
            {
                deck.Cards.Add(new Card(card.Suit, card.Rank, card.IsFaceUp));
            }

            standardDecks.Add(deck);
        }

        return standardDecks;
    }
}

public class ObservableStandardDeck : StandardDeck
{
    public ObservableStandardDeck(int seed = 42) : base(seed)
    {
        Cards.Clear();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                Cards.Add(new ObservableCard(suit, rank));
    }
}