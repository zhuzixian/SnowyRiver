using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class AuditedEntity<T> : HasCreationTimeEntity<T>, IAuditedEntity<T>, ISoftDelete
{
    public Guid? CreatorUserId { get; set; }
    public Guid? CreatorTeamId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierUserId { get; set; }
    public Guid? LastModifierTeamId { get; set; }
    public bool IsDeleted { get; set; }
}
