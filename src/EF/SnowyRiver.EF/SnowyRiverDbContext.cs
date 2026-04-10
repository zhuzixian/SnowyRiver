using Microsoft.EntityFrameworkCore;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.EF;

public class SnowyRiverDbContext(DbContextOptions options) : DbContext(options)
{
    protected virtual string DbTablePrefix => "";
    protected virtual string? DbSchema => null;

    protected virtual void ConfigEntityHistory<TEntity, TEntityId, TEntityHistory>(
        ModelBuilder modelBuilder, string tableName)
        where TEntityHistory : class, IEntityHistory<TEntity, TEntityId>
        where TEntity : IEntity<TEntityId>
    {
        modelBuilder.Entity<TEntityHistory>(b =>
        {
            b.ToTable(DbTablePrefix + tableName, DbSchema);
            b.HasKey(x => x.Id);
            b.Property(e => e.SnapShot).HasColumnType("json");
        });
    }

    protected virtual void EnsureAutoHistory<TEntity, TEntityId, TEntityHistory>()
        where TEntity : class, IEntity<TEntityId>
        where TEntityHistory : class, IEntityHistory<TEntity, TEntityId>, new()
    {
        var entries = ChangeTracker.Entries<TEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified
                or EntityState.Deleted)
            .ToList();

        foreach (var entityEntry in entries)
        {
            var entity = entityEntry.Entity;
            var history = new TEntityHistory
            {
                EntityId = entity.Id,
                Action = entityEntry.State.ToString(),
                SnapShot = entity,
                CreationTime = DateTime.Now,
            };
            Set<TEntityHistory>().Add(history);
        }
    }
}
