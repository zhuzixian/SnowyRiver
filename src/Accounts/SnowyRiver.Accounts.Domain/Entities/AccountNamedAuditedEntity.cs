namespace SnowyRiver.Accounts.Domain.Entities;

public class AccountNamedAuditedEntity<TUser, TTeam> 
    : SnowyRiver.Domain.Entities.NamedAuditedEntity<Guid, TUser, TTeam>
{
}
