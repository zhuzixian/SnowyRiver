namespace SnowyRiver.Accounts.Domain.Entities;

public class AccountAuditedEntity<TUser, TTeam> 
    : SnowyRiver.Domain.Entities.AuditedEntity<Guid, TUser, TTeam>
{
}
