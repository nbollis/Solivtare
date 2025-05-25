using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SolvitaireGUI;

public class PlayerToBrushConverter : BaseValueConverter<PlayerToBrushConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int player)
        {
            return player switch
            {
                0 => "White",
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
public class WinningCellToBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is int index && values[1] is HashSet<int> winningIndices)
        {
            return winningIndices.Contains(index) ? Brushes.Gold : Brushes.Blue;
        }
        return Brushes.Transparent; // Default brush if inputs are invalid  
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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

