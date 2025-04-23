namespace SolivtaireCore;

public class GameState
{
    public List<FoundationPile> FoundationPiles { get; } = new();
    public List<TableauPile> TableauPiles { get; } = new();
    public StockPile StockPile { get; } = new();
    public WastePile WastePile { get; } = new();

    public bool IsGameWon => FoundationPiles.All(pile => pile.Count == 13);

    public GameState()
    {
        for (int i = 0; i < 4; i++)
        {
            FoundationPiles.Add(new FoundationPile((Suit)i));
        }
        for (int i = 0; i < 7; i++)
        {
            TableauPiles.Add(new TableauPile());
        }
    }

    public void Reset()
    {
        foreach (var pile in FoundationPiles)
        {
            pile.Cards.Clear();
        }
        foreach (var pile in TableauPiles)
        {
            pile.Cards.Clear();
        }
        StockPile.Cards.Clear();
        WastePile.Cards.Clear();
    }

    public void DealCards(StandardDeck deck)
    {
        deck.Shuffle();
        Card[][] tableauCards = new Card[TableauPiles.Count][];
        for (int i = 0; i < TableauPiles.Count; i++)
        {
            tableauCards[i] = new Card[i + 1];
            for (int j = 0; j < tableauCards[i].Length; j++)
            {
                tableauCards[i][j] = deck.DrawCard();
            }
        }
        for (int i = 0; i < TableauPiles.Count; i++)
        {
            // cards.addrange here instead of add range so we do not trigger the validity check on deal
            TableauPiles[i].Cards.AddRange(tableauCards[i]);
            TableauPiles[i].TopCard.IsFaceUp = true;
        }

        while (deck.Cards.Count > 0)
        {
            StockPile.AddCard(deck.DrawCard());
        }
    }
}
