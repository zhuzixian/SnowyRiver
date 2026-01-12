using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Permission<TUser, TRole, TTeam, TPermission> : HasNameCreationTimeSoftDeleteEntity<Guid>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public List<TRole>? Roles { get; set; }
}
