using System.Globalization;
using SolvitaireCore;

namespace SolvitaireGUI;

public class TopCardConverter : BaseValueConverter<TopCardConverter>
{
    public override object  Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IList<Card> cards && cards.Count > 0)
            return cards[^1]; // C# index-from-end syntax

        return null;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}