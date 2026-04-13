using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class TeamAuditedEntity<TId, TUser, TTeam> 
    : AuditedEntity<TId, TUser, TTeam>, ITeamAuditedEntity<TId, TUser, TTeam>
{
    public TUser? User { get; set; }
    public TTeam? Team { get; set; }

    public Guid? UserId { get; set; }
    public Guid? TeamId { get; set; }
}
