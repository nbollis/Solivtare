using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SolvitaireGUI;

public class FlatIndexToColConverter : IValueConverter
{
    public int Columns { get; set; } = 3;
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i ? i % Columns : 0;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class FlatIndexToRowConverter : IValueConverter
{
    public int Rows { get; set; } = 3;
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i ? i / Rows : 0;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class PlayerToSymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            1 => "X",
            2 => "O",
            _ => ""
        };
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class CellBorderThicknessConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not int index || values[1] is not int boardSize)
            return new Thickness(2);

        int row = index / boardSize;
        int col = index % boardSize;

        // Thicker lines for the grid
        double thick = 4, thin = 2;
        double left = col == 0 ? thick : thin;
        double top = row == 0 ? thick : thin;
        double right = (col == boardSize - 1) ? thick : thin;
        double bottom = (row == boardSize - 1) ? thick : thin;

        return new Thickness(left, top, right, bottom);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
public class WinningCellToBrushTicTacConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is int index && values[1] is HashSet<int> winningIndices)
        {
            return winningIndices.Contains(index) ? Brushes.Khaki : Brushes.Transparent;
        }
        return Brushes.Transparent; // Default brush if inputs are invalid  
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}