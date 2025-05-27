using System.Globalization;

namespace SolvitaireGUI;

public class TakeLastConverter : BaseValueConverter<TakeLastConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<object> cards && int.TryParse(parameter?.ToString(), out int count))
            return cards.Reverse().Take(count).Reverse().ToList();

        return value;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}