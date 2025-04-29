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