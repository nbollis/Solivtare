namespace SolvitaireCore;

public abstract class Deck<TCard> where TCard : ICard
{
    protected Random Random { get; } = new Random();
    public List<TCard> Cards { get; private set; }

    protected Deck()
    {
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
        Random rng = new Random();
        int n = Cards.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (Cards[k], Cards[n]) = (Cards[n], Cards[k]);
        }
    }
}