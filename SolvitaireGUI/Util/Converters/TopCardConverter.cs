using System.Collections.ObjectModel;
using System.Globalization;
using SolvitaireCore;

namespace SolvitaireGUI;

public class TopCardConverter : BaseValueConverter<TopCardConverter>
{
    public override object  Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<Card> cards && cards.Any())
        {
            return cards.Last();
        }
        return null;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
