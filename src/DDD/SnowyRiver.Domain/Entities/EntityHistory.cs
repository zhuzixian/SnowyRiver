using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class EntityHistory<TEntity, TEntityId> : TeamAuditedEntity<TEntityId>, 
    IHistory<TEntity, TEntityId>
    where TEntity : IEntity<TEntityId>
{
    public TEntityId? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public TEntity? SnapShot { get; set; }
}
