using EntityFrameworkCore.Repository.Interfaces;
using System.Linq.Expressions;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace SnowyRiver.EF.DataAccess.Extensions;

public static class RepositoryExtensions
{
    extension<T>(IRepository<T> repository) where T : class
    {
        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            CancellationToken cancellationToken = default)
        {
            var query = repository.GetMultipleResultQuery(predicate, include);
            return await repository.FirstOrDefaultAsync(query, cancellationToken);
        }

        public async Task<T?> LastOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            CancellationToken cancellationToken = default)
        {
            var query = repository.GetMultipleResultQuery(predicate, include);
            return await repository.LastOrDefaultAsync(query, cancellationToken);
        }

        public async Task<T?> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            CancellationToken cancellationToken = default)
        {
            var query = repository.GetSingleResultQuery(predicate, include);
            return await repository.SingleOrDefaultAsync(query, cancellationToken);
        }

        public async Task<IList<T>> SearchAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            CancellationToken cancellationToken = default)
        {
            var query = repository.GetMultipleResultQuery(predicate, include);
            return await repository.SearchAsync(query, cancellationToken);
        }

        public IMultipleResultQuery<T> GetMultipleResultQuery(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            var query = repository.MultipleResultQuery().AndFilter(predicate);

            if (include != null)
            {
                query = query.Include(include);
            }

            return query;
        }

        public ISingleResultQuery<T> GetSingleResultQuery(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            var query = repository.SingleResultQuery().AndFilter(predicate);

            if (include != null)
            {
                query = query.Include(include);
            }

            return query;
        }
    }
}
