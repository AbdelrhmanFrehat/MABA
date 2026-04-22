using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MabaControlCenter.Converters;

public class BoolToSidebarWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isVisible = value is bool b && b;
        if (!isVisible)
            return new GridLength(0);

        var width = 220d;
        if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            width = parsed;

        return new GridLength(width);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
