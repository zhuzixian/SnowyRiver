using SnowyRiver.ComponentModel.Interface;

namespace SnowyRiver.Domain.Entities;
public class HasCreationTimeSoftDeleteEntity<TKey>:SoftDeleteEntity<TKey>, IHasCreationTime
{
    public DateTime CreationTime { get; set; }
}
