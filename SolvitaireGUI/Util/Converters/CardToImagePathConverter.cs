using SolvitaireCore;
using System.Globalization;

namespace SolvitaireGUI;

public class CardToImagePathConverter : BaseValueConverter<CardToImagePathConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Card card)
        {
            if (!card.IsFaceUp)
                return "/Resources/Cards/back.png";

            string rank = card.Rank switch
            {
                Rank.Ace => "A",
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                _ => ((int)card.Rank).ToString()
            };

            string suit = card.Suit switch
            {
                Suit.Clubs => "C",
                Suit.Diamonds => "D",
                Suit.Hearts => "H",
                Suit.Spades => "S",
                _ => "X"
            };

            return $"/Resources/Cards/{rank}{suit}.png";
        }
        return null;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}