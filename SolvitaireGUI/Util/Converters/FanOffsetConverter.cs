using System.Globalization;

namespace SolvitaireGUI;

public class FanOffsetConverter : BaseValueConverter<FanOffsetConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int index = (int)value;
        return index * 20; // 25 pixels vertical offset per card
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}