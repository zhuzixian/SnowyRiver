using System.ComponentModel;

namespace SnowyRiver.WPF.MarkupExtension;
public class EnumBindingSourceExtension : System.Windows.Markup.MarkupExtension
{
    private Type _enumType;
    public Type EnumType
    {
        get => _enumType;
        set
        {
            if (value != _enumType)
            {
                if (null != value)
                {
                    Type enumType = Nullable.GetUnderlyingType(value) ?? value;

                    if (!enumType.IsEnum)
                        throw new ArgumentException("Type must be for an Enum.");
                }

                _enumType = value;
            }
        }
    }

    public EnumBindingSourceExtension() { }

    public EnumBindingSourceExtension(Type enumType)
    {
        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (null == _enumType)
            throw new InvalidOperationException("The EnumType must be specified.");

        var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
        var enumValues = Enum.GetValues(actualEnumType);
        if (actualEnumType != _enumType)
        {
            var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            enumValues = tempArray;
        }

        return (from object enumValue in enumValues
                select new EnumerationMember
                {
                    Value = enumValue,
                    Description = GetDescription(enumValue)
                }).ToArray();
    }

    private string? GetDescription(object enumValue)
    {
        return EnumType
            .GetField(enumValue.ToString() ?? string.Empty)
            ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() is DescriptionAttribute descriptionAttribute
            ? descriptionAttribute.Description
            : enumValue.ToString();
    }

    public class EnumerationMember
    {
        public string? Description { get; set; } = default;
        public object Value { get; set; } = default;
    }
}
