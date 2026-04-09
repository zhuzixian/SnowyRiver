using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;

public class HasCreationTimeEntity<T> : Entity<T>,IHasCreationTime
{
    public DateTime CreationTime { get; set; }
}
