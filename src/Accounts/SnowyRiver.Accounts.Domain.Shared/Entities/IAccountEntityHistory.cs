using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Shared.Entities;

public interface IAccountEntityHistory<TEntity, TEntityId, TUser, TTeam> 
    : IAccountAuditedEntity<TUser, TTeam>,IEntityHistory<TEntity, TEntityId> 
    where TEntity : IEntity<TEntityId>
{
}
