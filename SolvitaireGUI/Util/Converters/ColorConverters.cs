using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SolvitaireGUI;

public class PlayerToBrushMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // value 0 is player number, 1 is player 1 color, 2 is player 2 color, 3 is background color (optional)
        if (values.Length < 3 || values[0] is not int value)
            return Brushes.Transparent;
        if (value == 1 && values[1] is Color p1)
            return new SolidColorBrush(p1);
        if (value == 2 && values[2] is Color p2)
            return new SolidColorBrush(p2);
        return Brushes.Transparent; // Empty slot
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class DrawingColorToWindowsColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Drawing.Color drawingColor)
        {
            return Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }
        return null;
    }
}


// Not used. 
public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color mediaColor)
        {
            return new SolidColorBrush(mediaColor);
        }
        return Brushes.Transparent; // Default to a non-null value  
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color;
        }
        return Colors.Transparent; // Default to a non-null value  
    }
}


