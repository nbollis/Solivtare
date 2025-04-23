using SolivtaireCore;

namespace Test;
internal class TestCard : ICard
{
    public Suit Suit { get; }
    public Rank Rank { get; }
    public bool IsFaceUp { get; set; }

    public TestCard(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public bool Equals(ICard? other) => other is TestCard card && Suit == card.Suit && Rank == card.Rank;
    public override bool Equals(object? obj) => Equals(obj as ICard);
    public override int GetHashCode() => HashCode.Combine(Suit, Rank);
}