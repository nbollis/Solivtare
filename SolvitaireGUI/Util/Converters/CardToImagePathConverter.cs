using SolvitaireCore;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SolvitaireGUI;

public class CardToImagePathMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not bool isFaceUp || values[1] is not Card card)
            return DependencyProperty.UnsetValue;

        string imagePath;
        if (!isFaceUp)
        {
            imagePath = "pack://application:,,,/Resources/Cards/back.png";
        }
        else
        {
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

            imagePath = $"pack://application:,,,/Resources/Cards/{rank}{suit}.png";
        }

        return new BitmapImage(new Uri(imagePath));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}