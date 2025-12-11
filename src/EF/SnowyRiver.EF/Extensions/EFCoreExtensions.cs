using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SnowyRiver.ComponentModel.Interface;

namespace SnowyRiver.EF.Extensions;
public static class EfCoreExtensions
{
    // 为每个实体设置全局“IsDeleted=false”查询筛选器
    public static void EnableSoftDeletionGlobalFilter(this ModelBuilder modelBuilder)
    {
        var entityTypesHasSoftDeletion = modelBuilder.Model.GetEntityTypes()
            .Where(e => e.ClrType.IsAssignableTo(typeof(ISoftDelete)));

        foreach (var entityType in entityTypesHasSoftDeletion)
        {
            var isDeletedProperty = entityType.FindProperty(nameof(ISoftDelete.IsDeleted));
            if (isDeletedProperty?.PropertyInfo != null)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "p");
                var filter = Expression.Lambda(
                    Expression.Not(Expression.Property(parameter, isDeletedProperty.PropertyInfo)),
                    parameter);
                entityType.SetQueryFilter(filter); ;
            }
        }
    }
}
