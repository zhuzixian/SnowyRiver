using EntityFrameworkCore.Repository.Interfaces;
using System.Linq.Expressions;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace SnowyRiver.EF.DataAccess.Extensions;

public static class RepositoryExtensions
{
    extension<T>(IRepository<T> repository) where T : class
    {
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = repository.GetMultipleResultQuery(predicate, includes);
            return await repository.FirstOrDefaultAsync(query, cancellationToken);
        }

        public async Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
                params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = repository.GetMultipleResultQuery(predicate, includes);
            return await repository.LastOrDefaultAsync(query, cancellationToken);
        }

        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = repository.GetSingleResultQuery(predicate, includes);
            return await repository.SingleOrDefaultAsync(query, cancellationToken);
        }

        public async Task<IList<T>> SearchAsync(Expression<Func<T, bool>> predicate, 
            CancellationToken cancellationToken = default,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = repository.GetMultipleResultQuery(predicate, includes);
            return await repository.SearchAsync(query, cancellationToken);
        }

        public IMultipleResultQuery<T> GetMultipleResultQuery(
            Expression<Func<T, bool>> predicate,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = includes.Aggregate(
                repository.MultipleResultQuery().AndFilter(predicate),
                (current, include) =>
                    current.Include(include));
            return query;
        }

        public ISingleResultQuery<T> GetSingleResultQuery(
            Expression<Func<T, bool>> predicate,
            params Func<IQueryable<T>, IIncludableQueryable<T, object>>[] includes)
        {
            var query = includes.Aggregate(
                repository.SingleResultQuery().AndFilter(predicate),
                (current, include) =>
                    current.Include(include));
            return query;
        }
    }

    
}
