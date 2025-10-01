using System.ComponentModel;

namespace SnowyRiver.WPF.MarkupExtension;
public class EnumerationDescriptionExtension(Type enumType) : System.Windows.Markup.MarkupExtension
{
    public Type EnumType { get; set; } = enumType;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var enumValues = Enum.GetValues(EnumType);
        return (from object? value in enumValues
            select new EnumerationMember
            {
                Value = value, Description = GetDescription(value) // 使用前面定义的GetDescription方法
            }).ToList();
    }

    private string? GetDescription(object? enumValue)
    {
        return EnumType
            .GetField((enumValue?.ToString() ?? string.Empty))
            ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() is DescriptionAttribute descriptionAttribute
            ? descriptionAttribute.Description
            : enumValue?.ToString();
    }
}

public class EnumerationMember
{
    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Value
    /// </summary>
    public object? Value { get; set; }
}
