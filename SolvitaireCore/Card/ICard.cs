namespace SolvitaireCore;

public record SerializableCard(Suit Suit, Rank Rank, bool IsFaceUp);
public interface ICard : IEquatable<ICard>
{
    Suit Suit { get; }
    Rank Rank { get; }
    bool IsFaceUp { get; set; }
}