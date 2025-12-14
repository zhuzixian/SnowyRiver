using SnowyRiver.ComponentModel.Interface;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;
public class SoftDeleteEntity<TKey>:Entity<TKey>,ISoftDelete
{
    public bool IsDeleted { get; set; }
}
