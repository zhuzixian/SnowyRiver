using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class TeamAuditedEntity<TId, TUser, TTeam> : AuditedEntity<TId>, ITeamAuditedEntity<TId>
{
    public TUser? User { get; set; }
    public TTeam? Team { get; set; }

    public Guid? UserId { get; set; }
    public Guid? TeamId { get; set; }
}
