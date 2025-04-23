namespace SolivtaireCore;
public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public static class SuitExtensions
{
    public static Color ToSuitColor(this Suit suit)
    {
        return suit switch
        {
            Suit.Hearts => Color.Red,
            Suit.Diamonds => Color.Red,
            Suit.Clubs => Color.Black,
            Suit.Spades => Color.Black,
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
        };
    }
}