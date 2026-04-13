using SnowyRiver.Accounts.Domain.Shared.Entities;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class AccountAuditedEntity<TUser, TRole, TTeam, TPermission> : TeamAuditedEntity<Guid, TUser, TTeam>,
    IAccountAuditedEntity<TUser, TTeam>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public TUser? CreatorUser { get; set; }
    public TTeam? CreatorTeam { get; set; }
    public TUser? LastModifierUser { get; set; }
    public TTeam? LastModifierTeam { get; set; }
}
