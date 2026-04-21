using EntityFrameworkCore.UnitOfWork.Interfaces;
using SnowyRiver.EF.Extensions;

namespace SnowyRiver.EF.DataAccess.Extensions;

public static class UnitOfWorkExtensions
{
    extension(IUnitOfWork unitOfWork)
    {
        public void AttachNavigations<TEntity>(TEntity? entity,
            params string[]? excludeProperties)
        {
            if(entity == null || excludeProperties == null)
            {
                return;
            }

            var type = entity.GetType();

            var entities = excludeProperties
                .Select(p => type.GetProperty(p)?.GetValue(entity))
                .Where(e => e is not null)
                .ToArray();

            foreach (var nav in entities)
            {
                unitOfWork.DbContext.AttachIfNotNull(nav);
            }
        }

        public void AttachNavigations<TEntity>(TEntity? entity)
            where TEntity : class
        {
            if(entity == null) return;

            var entry = unitOfWork.DbContext.Entry(entity);

            foreach (var navigation in entry.Navigations)
            {
                if (navigation.CurrentValue is not null)
                {
                    unitOfWork.DbContext.AttachIfNotNull(navigation.CurrentValue);
                }
            }
        }

        public void AttachNavigationsExcept<TEntity>(TEntity? entity,
            params string[] excludeProperties)
            where TEntity : class
        {
            if (entity == null)
            {
                return;
            }

            var excludeSet = excludeProperties?.ToHashSet(StringComparer.Ordinal)
                             ?? [];

            var entry = unitOfWork.DbContext.Entry(entity);

            foreach (var navigation in entry.Navigations)
            {
                if (!excludeSet.Contains(navigation.Metadata.Name)
                    && navigation.CurrentValue is not null)
                {
                    unitOfWork.DbContext.AttachIfNotNull(navigation.CurrentValue);
                }
            }
        }
    }
}
