namespace SnowyRiver.WPF.MarkupExtension;

/// <summary>
/// 为 WPF 绑定提供枚举值列表的数据源扩展。
/// </summary>
/// <remarks>
/// 典型用于 <see cref="System.Windows.Controls.ComboBox.ItemsSource"/>。
/// 当目标类型为可空枚举，或 <see cref="IncludeNullItem"/> 为 <see langword="true"/> 时，返回结果首项为 <see langword="null"/>。
/// </remarks>
[System.Windows.Markup.MarkupExtensionReturnType(typeof(object?[]))]
public class EnumValuesExtension : System.Windows.Markup.MarkupExtension
{
    private Type? _enumType;

    /// <summary>
    /// 获取或设置要绑定的枚举类型。
    /// </summary>
    /// <remarks>
    /// 支持可空枚举类型，如 <c>MyEnum?</c>。
    /// </remarks>
    public Type? EnumType
    {
        get => _enumType;
        set
        {
            if (value != _enumType)
            {
                if (null != value)
                {
                    _ = EnumBindingHelper.GetActualEnumType(value, nameof(EnumType));
                }

                _enumType = value;
            }
        }
    }

    /// <summary>
    /// 获取或设置是否包含空项。
    /// </summary>
    /// <remarks>
    /// 当为 <see langword="true"/> 时，在返回集合首位插入 <see langword="null"/>。
    /// </remarks>
    public bool IncludeNullItem { get; set; }

    /// <summary>
    /// 初始化 <see cref="EnumValuesExtension"/> 的新实例。
    /// </summary>
    public EnumValuesExtension() { }

    /// <summary>
    /// 使用指定枚举类型初始化 <see cref="EnumValuesExtension"/> 的新实例。
    /// </summary>
    /// <param name="enumType">枚举类型或可空枚举类型。</param>
    public EnumValuesExtension(Type enumType)
    {
        EnumType = enumType;
    }

    /// <summary>
    /// 返回用于绑定的枚举值集合。
    /// </summary>
    /// <param name="serviceProvider">XAML 服务提供器。</param>
    /// <returns>用于 <c>ItemsSource</c> 的枚举值数组。</returns>
    /// <exception cref="InvalidOperationException">未设置 <see cref="EnumType"/> 时抛出。</exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (_enumType is null)
            throw new InvalidOperationException("The EnumType must be specified.");

        return EnumBindingHelper.GetValues(_enumType, IncludeNullItem);
    }
}
