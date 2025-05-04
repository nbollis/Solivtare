using System.Globalization;

namespace SolvitaireGUI;

public class BooleanToAngleConverter : BaseValueConverter<BooleanToAngleConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked)
        {
            return isChecked ? 90 : 0; // Rotate 90 degrees when expanded
        }
        return 0;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}