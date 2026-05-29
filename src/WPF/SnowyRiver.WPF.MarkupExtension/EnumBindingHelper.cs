using System.Collections.Concurrent;
using System.ComponentModel;

namespace SnowyRiver.WPF.MarkupExtension;

/// <summary>
/// 枚举绑定内部帮助器，负责类型校验、值与描述缓存。
/// </summary>
internal static class EnumBindingHelper
{
    private static readonly ConcurrentDictionary<Type, object?[]> EnumValuesCache = new();
    private static readonly ConcurrentDictionary<(Type EnumType, object Value), string?> DescriptionCache = new();

    /// <summary>
    /// 获取实际枚举类型（去除可空包装）并校验类型有效性。
    /// </summary>
    /// <param name="enumType">枚举类型或可空枚举类型。</param>
    /// <param name="parameterName">异常参数名。</param>
    /// <returns>实际枚举类型。</returns>
    internal static Type GetActualEnumType(Type enumType, string parameterName)
    {
        var actualEnumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
        if (!actualEnumType.IsEnum)
            throw new ArgumentException("Type must be for an Enum.", parameterName);

        return actualEnumType;
    }

    /// <summary>
    /// 获取用于绑定的枚举值集合。
    /// </summary>
    /// <param name="enumType">枚举类型或可空枚举类型。</param>
    /// <param name="includeNullItem">是否强制包含空项。</param>
    /// <returns>枚举值数组；需要时首项为 <see langword="null"/>。</returns>
    internal static object?[] GetValues(Type enumType, bool includeNullItem)
    {
        var actualEnumType = GetActualEnumType(enumType, nameof(enumType));
        var cachedValues = EnumValuesCache.GetOrAdd(actualEnumType, static type =>
        {
            var enumValues = Enum.GetValues(type);
            var values = new object?[enumValues.Length];
            for (var i = 0; i < enumValues.Length; i++)
            {
                values[i] = enumValues.GetValue(i);
            }

            return values;
        });

        var isNullableEnum = Nullable.GetUnderlyingType(enumType) is not null;
        if (!isNullableEnum && !includeNullItem)
            return cachedValues;

        var result = new object?[cachedValues.Length + 1];
        cachedValues.CopyTo(result, 1);
        return result;
    }

    /// <summary>
    /// 获取枚举值对应的描述文本，优先读取 <see cref="DescriptionAttribute"/>。
    /// </summary>
    /// <param name="actualEnumType">实际枚举类型。</param>
    /// <param name="enumValue">枚举值。</param>
    /// <returns>描述文本；无特性时返回枚举名称。</returns>
    internal static string? GetDescription(Type actualEnumType, object enumValue)
    {
        return DescriptionCache.GetOrAdd((actualEnumType, enumValue), static key =>
        {
            var field = key.EnumType.GetField(key.Value.ToString() ?? string.Empty);
            if (field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute attr)
                return attr.Description;

            return key.Value.ToString();
        });
    }
}
