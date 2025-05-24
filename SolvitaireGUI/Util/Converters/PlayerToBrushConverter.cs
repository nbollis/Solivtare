using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace SolvitaireGUI;

public class PlayerToBrushConverter : BaseValueConverter<PlayerToBrushConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int player)
        {
            return player switch
            {
                0 => "Transparent",
                1 => "Red",
                2 => "Yellow",
                _ => "Transparent" // Default for invalid player values  
            };
        }
        return "Transparent"; // Default color if not a valid player  
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FlatIndexToColumnConverter : BaseValueConverter<FlatIndexToColumnConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
            if (value is int index)
                return index % 7; // 7 columns
            return 0;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PreviewTokenVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        int hovered = (int)values[0];
        int column = (int)values[1];
        return hovered == column ? Visibility.Visible : Visibility.Collapsed;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class PlayerTypeAgentModeToVisibilityConverter : BaseValueConverter<PlayerTypeAgentModeToVisibilityConverter>
{

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PlayerType mode)
        {
            return mode == PlayerType.Agent ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed; // Default visibility if not a valid mode
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PlayerTypeHumanModeToVisibilityConverter : BaseValueConverter<PlayerTypeHumanModeToVisibilityConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PlayerType mode)
        {
            return mode == PlayerType.Human ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed; // Default visibility if not a valid mode
    }
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
