using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class TeamAuditedEntity<T> : AuditedEntity<T>, ITeamAuditedEntity<T>
{
    public Guid? UserId { get; set; }
    public Guid? TeamId { get; set; }
}
