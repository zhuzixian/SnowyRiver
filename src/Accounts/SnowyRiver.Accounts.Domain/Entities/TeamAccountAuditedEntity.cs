using SnowyRiver.Accounts.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class TeamAccountAuditedEntity<TUser, TRole, TTeam, TPermission> : AccountAuditedEntity<TUser, TRole, TTeam, TPermission>, 
    ITeamAccountAuditedEntity<TUser, TTeam>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public TUser? User { get; set; }
    public TTeam? Team { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TeamId { get; set; }
}
