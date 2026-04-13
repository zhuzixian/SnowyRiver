using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class NamedAuditedEntity<TId, TUser, TTeam> 
    : AuditedEntity<TId, TUser, TTeam>,
        IHasName
{
    public string? Name { get; set; }
}
