using System.Globalization;
using System.Windows.Data;

namespace SnowyRiver.WPF.Controls.Injector.Converters;
public class InjectionPumpPistonPositionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var top = 0d;
        if (values is [double height, double pistonSize, double minimum, double maximum, double value, ..])
        {
            top =  (height - pistonSize) / (maximum - minimum) * value;
        }

        return top;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
