using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class User<TUser, TRole, TTeam, TPermission> : HasNameCreationTimeSoftDeleteEntity<Guid>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public int UserId { get; set; }

    public string Password { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public List<TRole> Roles { get; set; } = [];
    public List<TTeam> Teams { get; set; } = [];
}
