using System.Globalization;
using System.Windows;

namespace SolvitaireGUI;

public class AgentPanelConverters : BaseValueConverter<AgentPanelConverters>
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