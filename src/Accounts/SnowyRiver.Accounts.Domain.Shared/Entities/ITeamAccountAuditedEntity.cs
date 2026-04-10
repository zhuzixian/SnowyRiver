
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Shared.Entities;

public interface ITeamAccountAuditedEntity<TUser, TTeam>
    : IAccountAuditedEntity<TUser, TTeam>,ITeamAuditedEntity<Guid>
{
    public TUser? User { get; set; }
    public TTeam? Team { get; set; }
}
