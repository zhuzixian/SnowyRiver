using SnowyRiver.ComponentModel.Interface;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Domain.Entities;
public class HasNameCreationTimeSoftDeleteEntity<TKey> : HasCreationTimeSoftDeleteEntity<TKey>,IHasName
{
    public string Name { get; set; } = string.Empty;
}
