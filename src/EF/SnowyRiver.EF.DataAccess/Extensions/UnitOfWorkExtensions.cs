using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using SnowyRiver.EF.Extensions;

namespace SnowyRiver.EF.DataAccess.Extensions;

public static class UnitOfWorkExtensions
{
    extension(IUnitOfWork unitOfWork)
    {
        public async Task<TEntity> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            unitOfWork.AttachNavigationsIfNotDefaultKey(entity);

            var repository = unitOfWork.Repository<TEntity>();
            return await repository.AddAsync(entity, cancellationToken);
        }

        public void AttachNavigationsIfNotDefaultKey<TEntity>(TEntity? entity)
            where TEntity : class
        {
            if (entity == null) return;

            var processedEntities = new HashSet<object>(ReferenceEqualityComparer.Instance);
            unitOfWork.AttachNavigationsIfNotDefaultKeyRecursive(entity, processedEntities);
        }

        private void AttachNavigationsIfNotDefaultKeyRecursive(object? entity,
            HashSet<object> processedEntities)
        {
            if (entity == null || !processedEntities.Add(entity))
            {
                return;
            }

            try
            {
                var entry = unitOfWork.DbContext.Entry(entity);

                // 处理导航属性
                foreach (var navigation in entry.Navigations.Where(x => !x.Metadata.IsCollection))
                {
                    unitOfWork.AttachIfNotDefaultKey(navigation.CurrentValue);
                    if (navigation.CurrentValue != null)
                    {
                        // 递归处理导航属性的导航属性
                        unitOfWork.AttachNavigationsIfNotDefaultKeyRecursive(navigation.CurrentValue, processedEntities);
                    }
                }

                // 处理集合导航属性
                foreach (var collection in entry.Collections)
                {
                    if (collection.CurrentValue == null)
                    {
                        continue;
                    }

                    foreach (var item in collection.CurrentValue)
                    {
                        unitOfWork.AttachIfNotDefaultKey(item);
                        // 递归处理集合项的导航属性
                        unitOfWork.AttachNavigationsIfNotDefaultKeyRecursive(item, processedEntities);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // 实体可能不是 EF 跟踪的类型，跳过
            }
        }

        private void AttachIfNotDefaultKey(object? entity)
        {
            if (entity == null)
            {
                return;
            }

            var navEntry = unitOfWork.DbContext.Entry(entity);

            // 获取导航实体的主键属性
            var primaryKey = navEntry.Metadata.FindPrimaryKey();
            if (primaryKey == null)
                return;

            // 检查主键是否为默认值
            var isDefaultKey = navEntry.IsDefaultKeyValue(primaryKey);

            // 仅当主键不为默认值时才附加
            if (!isDefaultKey && navEntry.State == EntityState.Detached)
            {
                unitOfWork.DbContext.Attach(entity);
            }
        }
    }
}
