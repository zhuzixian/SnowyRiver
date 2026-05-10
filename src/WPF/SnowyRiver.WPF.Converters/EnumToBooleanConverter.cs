using System.Globalization;
using System.Windows.Data;

namespace SnowyRiver.WPF.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null || value == null) return false;
        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null) return Binding.DoNothing;
        var isChecked = value as bool? ?? false;
        if (!isChecked) return Binding.DoNothing;

        // 支持 Nullable<Enum>
        var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (!enumType.IsEnum) return Binding.DoNothing;

        try
        {
            return Enum.Parse(enumType, parameter.ToString() ?? string.Empty, ignoreCase: true);
        }
        catch
        {
            return Binding.DoNothing;
        }
    }
}
