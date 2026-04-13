using SnowyRiver.Accounts.Domain.Shared.Entities;
using SnowyRiver.Domain.Entities;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class AccountEntityHistory<TEntity, TEntityId, TUser, TRole, TTeam, TPermission>
    : EntityHistory<TEntity, TEntityId>,IAccountAuditedEntity<TUser, TTeam>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TEntity : IEntity<TEntityId>
{
    public TUser? User { get; set; }
    public TTeam? Team { get; set; }
    public TUser? CreatorUser { get; set; }
    public TTeam? CreatorTeam { get; set; }
    public TUser? LastModifierUser { get; set; }
    public TTeam? LastModifierTeam { get; set; }
}
