using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class PermissionHistory<TUser, TRole, TTeam, TPermission> : EntityHistory<TPermission, Guid>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
}
