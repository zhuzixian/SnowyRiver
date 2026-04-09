using EntityFrameworkCore.Repository.Interfaces;
using System.Linq.Expressions;

namespace SnowyRiver.EF.DataAccess.Extensions;

public static class RepositoryExtensions
{
    extension<T>(IRepository<T> repository) where T : class
    {
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await repository.FirstOrDefaultAsync(repository.MultipleResultQuery()
                .AndFilter(predicate), cancellationToken);
        }

        public async Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await repository.LastOrDefaultAsync(repository.MultipleResultQuery()
                .AndFilter(predicate), cancellationToken);
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate,
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
}
