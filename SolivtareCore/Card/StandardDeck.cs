using SolivtaireCore;

namespace SolivtaireCore;

public class StandardDeck : Deck<Card>
{
    public StandardDeck()
    {
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                Cards.Add(new Card(suit, rank));
    }
}