using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Shared.Entities;

public interface IAccountAuditedEntity<TUser, TTeam>: IAuditedEntity<Guid>
{
    public TUser? CreatorUser { get; set; }
    public TTeam? CreatorTeam { get; set; }
    public TUser? LastModifierUser { get; set; }
    public TTeam? LastModifierTeam { get; set; }
}
