using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class AccountEntityHistory<TEntity, TUser, TTeam> : SnowyRiver.Domain.Entities.EntityHistory<TEntity, Guid, TUser, TTeam>
    where TEntity : IEntity<Guid>
{
}
