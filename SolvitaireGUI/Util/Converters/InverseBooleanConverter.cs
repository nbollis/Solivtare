using System.Globalization;

namespace SolvitaireGUI;

public class InverseBooleanConverter : BaseValueConverter<InverseBooleanConverter>
{

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value;
    }
}