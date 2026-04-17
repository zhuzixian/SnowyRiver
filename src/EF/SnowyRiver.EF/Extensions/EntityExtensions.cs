using Microsoft.EntityFrameworkCore;

namespace SnowyRiver.EF.Extensions;

public static class EntityExtensions
{
    public static TEntity DetachNavigations<TEntity>(this TEntity entity, DbContext context)
        where TEntity : class
    {
        var entry = context.Entry(entity);

        // 将所有导航属性的引用设为未加载状态
        foreach (var navigation in entry.Navigations)
        {
            navigation.CurrentValue = null;
        }

        return entity;
    }
}
