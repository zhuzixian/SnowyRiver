using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace SnowyRiver.EF.Extensions;

public static class DbContextExtensions
{
    extension(DbContext context)
    {
        /// <summary>
        /// 批量附加导航属性实体，避免重复更新
        /// </summary>
        public void AttachIfNotNull(params object?[] entities)
        {
            foreach (var entity in entities)
            {
                if (entity != null)
                {
                    context.Attach(entity);
                }
            }
        }

        /// <summary>
        /// 自动附加实体的所有导航属性
        /// </summary>
        public void AttachNavigationProperties<T>(T? entity, params string[] excludeProperties) 
            where T : class
        {
            if (entity == null) return;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p is { CanRead: true, PropertyType.IsClass: true } &&
                            p.PropertyType != typeof(string) &&
                            !excludeProperties.Contains(p.Name));

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                if (value != null && !context.Entry(value).IsKeySet)
                {
                    context.Attach(value);
                }
            }
        }
    }
}
