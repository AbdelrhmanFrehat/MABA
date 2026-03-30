using System.Globalization;
using System.Windows.Data;
using MabaControlCenter.Localization;

namespace MabaControlCenter.Converters;

/// <summary>
/// Converts bool to localized Yes/No. Use with MultiBinding: (bool value, LocalizedLabels).
/// When culture changes, Labels updates and the binding re-evaluates.
/// </summary>
public class BoolToLocalizedYesNoConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var isYes = values.Length > 0 && values[0] is true;
        if (values.Length > 1 && values[1] is LocalizedLabels labels)
            return isYes ? labels.Label_Yes : labels.Label_No;
        return isYes ? "Yes" : "No";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
