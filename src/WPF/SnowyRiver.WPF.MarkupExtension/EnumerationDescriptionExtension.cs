namespace SnowyRiver.WPF.MarkupExtension;

/// <summary>
/// 为 WPF 绑定提供包含枚举值与描述文本的集合。
/// </summary>
/// <remarks>
/// 返回项类型为 <see cref="EnumerationMember"/>，通常与
/// <c>DisplayMemberPath="Description"</c>、<c>SelectedValuePath="Value"</c> 配合使用。
/// </remarks>
[System.Windows.Markup.MarkupExtensionReturnType(typeof(IReadOnlyList<EnumerationMember>))]
public class EnumerationDescriptionExtension : System.Windows.Markup.MarkupExtension
{
    /// <summary>
    /// 初始化 <see cref="EnumerationDescriptionExtension"/> 的新实例。
    /// </summary>
    public EnumerationDescriptionExtension() { }

    /// <summary>
    /// 使用指定枚举类型初始化 <see cref="EnumerationDescriptionExtension"/> 的新实例。
    /// </summary>
    /// <param name="enumType">枚举类型或可空枚举类型。</param>
    public EnumerationDescriptionExtension(Type enumType)
    {
        EnumType = enumType;
    }

    /// <summary>
    /// 获取或设置要绑定的枚举类型。
    /// </summary>
    public Type? EnumType { get; set; }

    /// <summary>
    /// 获取或设置是否包含空项。
    /// </summary>
    public bool IncludeNullItem { get; set; }

    /// <summary>
    /// 获取或设置空项显示文本。
    /// </summary>
    public string NullItemDescription { get; set; } = string.Empty;

    /// <summary>
    /// 返回包含枚举值与描述文本的集合。
    /// </summary>
    /// <param name="serviceProvider">XAML 服务提供器。</param>
    /// <returns><see cref="EnumerationMember"/> 列表。</returns>
    /// <exception cref="InvalidOperationException">未设置 <see cref="EnumType"/> 时抛出。</exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (EnumType is null)
            throw new InvalidOperationException("The EnumType must be specified.");

        var actualEnumType = EnumBindingHelper.GetActualEnumType(EnumType, nameof(EnumType));
        var values = EnumBindingHelper.GetValues(EnumType, IncludeNullItem);

        return values
            .Select(value => new EnumerationMember
            {
                Value = value,
                Description = value is null
                    ? NullItemDescription
                    : EnumBindingHelper.GetDescription(actualEnumType, value)
            })
            .ToList();
    }
}

/// <summary>
/// 枚举绑定项。
/// </summary>
public class EnumerationMember
{
    /// <summary>
    /// 显示文本。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 枚举值。
    /// </summary>
    public object? Value { get; set; }
}
