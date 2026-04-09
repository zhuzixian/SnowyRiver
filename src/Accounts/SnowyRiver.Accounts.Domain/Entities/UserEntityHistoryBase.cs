using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class UserEntityHistoryBase<TEntity, TEntityId>
    : UserAssociationEntityBase, IEntityHistory<TEntity, TEntityId> 
    where TEntity : IEntity<TEntityId>
{
    public TEntityId? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public TEntity? SnapShot { get; set; }
}
