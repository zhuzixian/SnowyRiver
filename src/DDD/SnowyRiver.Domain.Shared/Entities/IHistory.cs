namespace SnowyRiver.Domain.Shared.Entities;

public interface IHistory<TEntity, TEntityId> : IAudited
    where TEntity : IEntity<TEntityId>
{
    TEntityId? EntityId { get; set; }
    string Action { get; set; } // Created/Updated/Deleted
    TEntity? SnapShot { get; set; }
}
