using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class UserEntityHistory<TEntity, TEntityId, TUser, TRole, TTeam, TPermission>
    : UserEntityHistoryBase<TEntity, TEntityId>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TEntity : IEntity<TEntityId>
{
    public TUser? User { get; set; }
    public TTeam? Team { get; set; }
}
