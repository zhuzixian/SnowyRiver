using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace SnowyRiver.WPF.Converters;
    public class EnumDescriptionTypeConverter(Type type) : EnumConverter(type), IValueConverter
    {
        private static string GetEnumDescription(Enum enumObj)
        {
            var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            var attributesArray = fieldInfo.GetCustomAttributes(false);

            if (attributesArray.Length == 0)
            {
                return enumObj.ToString();
            }

            if (attributesArray[0] is DescriptionAttribute attrib) return attrib.Description;
            return "";
        }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    var fieldInfo = value.GetType().GetField(value.ToString());
                    if (fieldInfo != null)
                    {
                        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        return attributes.Length > 0 && !string.IsNullOrEmpty(attributes[0].Description) ? attributes[0].Description : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            var myEnum = (Enum)value; 
            var description = GetEnumDescription(myEnum);
            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            throw new NotImplementedException();
        }
    }
