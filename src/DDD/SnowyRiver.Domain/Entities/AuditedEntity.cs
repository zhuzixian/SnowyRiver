using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class AuditedEntity<TId, TUser, TTeam> 
    : HasCreationTimeEntity<TId>,
        IAudited<TUser, TTeam>, ISoftDelete
{
    public Guid? CreatorUserId { get; set; }
    public Guid? CreatorTeamId { get; set; }
    public Guid? LastModifierUserId { get; set; }
    public Guid? LastModifierTeamId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public bool IsDeleted { get; set; }

    public TUser? CreatorUser { get; set; }
    public TTeam? CreatorTeam { get; set; }
    public TUser? LastModifierUser { get; set; }
    public TTeam? LastModifierTeam { get; set; }
}
