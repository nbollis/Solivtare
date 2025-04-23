namespace SolivtaireCore;

public interface ICard : IEquatable<ICard>
{
    Suit Suit { get; }
    Rank Rank { get; }
    bool IsFaceUp { get; set; }
}