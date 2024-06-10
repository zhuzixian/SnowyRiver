using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SnowyRiver.WPF.Controls.Valves.Converters
{
    internal class ActualWidthToTrianglePointsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double actualWidth && values[1] is double actualHeight)
            {
                var halfWidth = actualWidth / 2;
                var halfHeight = actualHeight / 2;
                if (parameter is Dock.Top)
                {
                    return new PointCollection{
                        new(0, 0),
                        new(halfWidth, halfHeight),
                        new(actualWidth, 0)
                    };
                }
                if (parameter is  Dock.Bottom)
                {
                    return new PointCollection
                    {
                        new(0, actualHeight),
                        new(halfWidth, halfHeight),
                        new(actualWidth, actualHeight)
                    };
                }
                if (parameter is Dock.Left)
                {
                    return new PointCollection
                    {
                        new(0, 0),
                        new(halfWidth, halfHeight),
                        new(0, actualHeight)
                    };
                }
                if (parameter is Dock.Right)
                {
                    return new PointCollection
                    {
                        new(actualWidth, 0),
                        new(halfWidth, halfHeight),
                        new(actualWidth, actualHeight)
                    };
                }
            }
            
            return new PointCollection();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
