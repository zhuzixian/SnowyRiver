using EntityFrameworkCore.Repository.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SnowyRiver.EF.Extensions;

namespace SnowyRiver.EF.DataAccess.Extensions;

public static class RepositoryExtensions
{
    extension<T>(IRepository<T> repository) where T : class
    {
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await repository.FirstOrDefaultAsync(
                repository.MultipleResultQuery()
                .AndFilter(predicate), cancellationToken);
        }

        public async Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await repository.LastOrDefaultAsync(repository.MultipleResultQuery()
                .AndFilter(predicate), cancellationToken);
        }

        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await repository.SingleOrDefaultAsync(repository.SingleResultQuery()
                .AndFilter(predicate), cancellationToken);
        }

        public async Task<IList<T>> SearchAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await repository.SearchAsync(repository.MultipleResultQuery()
                .AndFilter(predicate), cancellationToken);
        }
    }

    public static async Task<TEntity> AddWithoutNavigationsAsync<TEntity>(
        this IRepository<TEntity> repository,
        TEntity entity,
        DbContext context,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        entity.DetachNavigations(context);
        return await repository.AddAsync(entity, cancellationToken);
    }
}
