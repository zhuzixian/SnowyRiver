using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SnowyRiver.WPF.Converters;

public class ObjectToGridLengthStarConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return new GridLength(intValue, GridUnitType.Star);
        }
        
        if (value is double doubleValue)
        {
            return new GridLength(doubleValue, GridUnitType.Star);
        }

        return new GridLength(1, GridUnitType.Star);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GridLength { GridUnitType: GridUnitType.Star } gridLength)
        {
            return (int)gridLength.Value;
        }

        return 1;
    }
}
