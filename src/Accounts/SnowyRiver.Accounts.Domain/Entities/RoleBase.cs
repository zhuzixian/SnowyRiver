using SnowyRiver.Accounts.Domain.Shared;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Role<TUser, TRole, TTeam, TPermission> : HasNameCreationTimeSoftDeleteEntity<Guid>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    /// <summary>
    /// 权限范围
    /// </summary>
    public PermissionsScope Scope { get; set; }
    public List<TPermission> Permissions { get; set; }
    public List<TUser> Users { get; set; }
}
