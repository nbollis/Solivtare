using System.ComponentModel;

namespace SolvitaireCore;

/// <summary>
/// Represents a minimalized card object for game logic.
/// </summary>
public class Card(Suit suit, Rank rank, bool isFaceUp = false) : ICard
{
    public virtual Suit Suit { get; } = suit;
    public virtual Rank Rank { get; } = rank;
    public Color Color { get; } = suit.ToSuitColor();
    public virtual bool IsFaceUp { get; set; } = isFaceUp;

    public override string ToString() => $"{Rank}{Suit.ToSuitCharacter()}";

    public bool Equals(ICard? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Suit == other.Suit && Rank == other.Rank;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is ICard card && Equals(card);
    }

    public override int GetHashCode() => HashCode.Combine((int)Suit, (int)Rank);

    public Card Clone() => new(Suit, Rank, IsFaceUp);
}

public class ObservableCard(Suit suit, Rank rank, bool isFaceUp = false)
    : Card(suit, rank, isFaceUp), INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override bool IsFaceUp
    {
        get => base.IsFaceUp;
        set
        {
            if (base.IsFaceUp != value)
            {
                base.IsFaceUp = value;
                OnPropertyChanged(nameof(IsFaceUp));
            }
        }
    } 
}