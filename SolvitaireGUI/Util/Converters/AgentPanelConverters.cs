﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SolvitaireGUI;

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

public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value != null;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}