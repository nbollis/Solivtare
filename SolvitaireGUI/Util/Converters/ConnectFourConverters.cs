using System.Collections;
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
public class ItemToIndexConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var item = values[0];
        var collection = values[1] as IEnumerable;
        if (collection == null) return -1;
        int index = 0;
        foreach (var obj in collection)
        {
            if (Equals(obj, item)) return index;
            index++;
        }
        return -1;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class IsLastMoveCellConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] = item, values[1] = collection, values[2] = LastMoveFlatIndex
        if (values.Length < 3) return false;
        var item = values[0];
        var collection = values[1] as IEnumerable;
        if (collection == null || values[2] is not int lastMoveIndex) return false;
        int index = 0;
        foreach (var obj in collection)
        {
            if (Equals(obj, item))
                return index == lastMoveIndex;
            index++;
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
