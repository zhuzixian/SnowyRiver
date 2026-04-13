using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class AccountTeamAuditedEntity<TUser, TTeam> 
    : TeamAuditedEntity<Guid, TUser, TTeam>
{
}
