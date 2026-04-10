using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class NamedAuditedEntity<T> : AuditedEntity<T>, IHasName
{
    public string? Name { get; set; }
}
