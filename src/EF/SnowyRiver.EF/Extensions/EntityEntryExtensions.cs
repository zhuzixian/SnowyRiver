using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SnowyRiver.EF.Extensions;

public static class EntityEntryExtensions
{
    /// <summary>
    /// 检查实体的主键是否为默认值
    /// </summary>
    public static bool IsDefaultKeyValue(this EntityEntry navEntry, IKey primaryKey)
    {
        foreach (var property in primaryKey.Properties)
        {
            var value = navEntry.Property(property.Name).CurrentValue;

            // 检查是否为默认值
            if (value == null)
                return true;

            var propertyType = property.ClrType;
            var defaultValue = propertyType.IsValueType
                ? Activator.CreateInstance(propertyType)
                : null;

            // 比较当前值与默认值
            if (!Equals(value, defaultValue))
            {
                return false; // 有一个主键不是默认值，说明是已存在实体
            }
        }

        return true; // 所有主键都是默认值
    }
}
