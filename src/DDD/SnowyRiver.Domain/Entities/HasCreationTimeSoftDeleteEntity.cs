using System;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;
public class HasCreationTimeSoftDeleteEntity<TKey>:SoftDeleteEntity<TKey>, IHasCreationTime
{
    public DateTime CreationTime { get; set; }
}
