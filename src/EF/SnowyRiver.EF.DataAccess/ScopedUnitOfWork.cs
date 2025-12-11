using System.Data.Common;
using System.Transactions;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using IsolationLevel = System.Data.IsolationLevel;

namespace SnowyRiver.EF.DataAccess;

public class ScopedUnitOfWork : IUnitOfWork
{
    private readonly IServiceScope _serviceScope;
    private readonly IUnitOfWork _innerUnitOfWork;

    public ScopedUnitOfWork(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _innerUnitOfWork = _serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    public void Dispose()
    {
        _innerUnitOfWork.Dispose();
        _serviceScope.Dispose();
    }

    public T CustomRepository<T>() where T : class => _innerUnitOfWork.CustomRepository<T>();
    public IRepository<T> Repository<T>() where T : class => _innerUnitOfWork.Repository<T>();
    public bool HasTransaction() => _innerUnitOfWork.HasTransaction();
    public bool HasChanges() => _innerUnitOfWork.HasChanges();

    public int SaveChanges(bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false)
        => _innerUnitOfWork.SaveChanges(acceptAllChangesOnSuccess, ensureAutoHistory);

    public void DiscardChanges() => _innerUnitOfWork.DiscardChanges();

    public IExecutionStrategy CreateExecutionStrategy() => _innerUnitOfWork.CreateExecutionStrategy();

    public void UseTransaction(DbTransaction transaction, Guid? transactionId = null)
        => _innerUnitOfWork.UseTransaction(transaction, transactionId);

    public void EnlistTransaction(Transaction transaction)
        => _innerUnitOfWork.EnlistTransaction(transaction);

    public Transaction GetEnlistedTransaction()
        => _innerUnitOfWork.GetEnlistedTransaction();

    public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        => _innerUnitOfWork.BeginTransaction();

    public void Commit() => _innerUnitOfWork.Commit();

    public void Rollback() => _innerUnitOfWork.Rollback();

    public int ExecuteSqlCommand(string sql, params object[] parameters) 
        => _innerUnitOfWork.ExecuteSqlCommand(sql, parameters);

    public IList<T> FromSql<T>(string sql, params object[] parameters) where T : class
        => _innerUnitOfWork.FromSql<T>(sql, parameters);

    public void ChangeDatabase(string database)
        => _innerUnitOfWork.ChangeDatabase(database);

    public void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback)
        => _innerUnitOfWork.TrackGraph(rootEntity, callback);

    public void TrackGraph<TState>(object rootEntity, TState state, Func<EntityEntryGraphNode<TState>, bool> callback)
        => _innerUnitOfWork.TrackGraph(rootEntity, state, callback);

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, bool ensureAutoHistory = false,
        CancellationToken cancellationToken = new())
        => _innerUnitOfWork.SaveChangesAsync(acceptAllChangesOnSuccess, ensureAutoHistory, cancellationToken);

    public Task UseTransactionAsync(DbTransaction transaction, Guid? transactionId = null,
        CancellationToken cancellationToken = new())
        => _innerUnitOfWork.UseTransactionAsync(transaction, transactionId, cancellationToken);

    public Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = new())
        => _innerUnitOfWork.BeginTransactionAsync(isolationLevel, cancellationToken);

    public Task CommitAsync(CancellationToken cancellationToken = new())
        => _innerUnitOfWork.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = new())
        => _innerUnitOfWork.RollbackAsync(cancellationToken);

    public Task<IList<T>> FromSqlAsync<T>(string sql, IEnumerable<object> parameters = null,
        CancellationToken cancellationToken = new()) where T : class
        => _innerUnitOfWork.FromSqlAsync<T>(sql, parameters, cancellationToken);

    public Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters = null,
        CancellationToken cancellationToken = new())
        => _innerUnitOfWork.ExecuteSqlCommandAsync(sql, parameters, cancellationToken);

    public DbContext DbContext => _innerUnitOfWork.DbContext;

    public TimeSpan? Timeout
    {
        get => _innerUnitOfWork.Timeout; 
        set => _innerUnitOfWork.Timeout = value;
    }
}
