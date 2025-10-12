using SnowyRiver.ComponentModel.Interface;

namespace SnowyRiver.Domain.Entities;
public class SoftDeleteEntity<TKey>:Entity<TKey>,ISoftDelete
{
    public bool IsDeleted { get; set; }
}
