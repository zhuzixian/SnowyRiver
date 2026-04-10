using SnowyRiver.Accounts.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class NamedAccountAuditedEntity<TUser, TRole, TTeam, TPermission> 
    : AccountAuditedEntity<
    TUser, TRole, TTeam, TPermission>, INamedAccountAuditedEntity<TUser, TTeam>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public string? Name { get; set; }
}
