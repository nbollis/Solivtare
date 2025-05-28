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

        if (values.Length < 3) return Brushes.Black;
        int player = values[0] is int p ? p : 0;
        var player1Color = values[1] as Color? ?? Colors.Blue;
        var player2Color = values[2] as Color? ?? Colors.Red;

        // Bind default background color to App.xaml resource  
        var backgroundBrush = values.Length > 3
            ? values[3] switch
            {
                Brush brush => brush,
                Color color => new SolidColorBrush(color),
                _ => Application.Current.Resources["BackgroundColor"] as Brush
            }
            : Application.Current.Resources["BackgroundColor"] as Brush;

        var player1Brush = new SolidColorBrush(player1Color);
        var player2Brush = new SolidColorBrush(player2Color);

        return player switch
        {
            0 => backgroundBrush,
            1 => player1Brush,
            2 => player2Brush,
            _ => Brushes.Black
        } ?? Brushes.Black;
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


