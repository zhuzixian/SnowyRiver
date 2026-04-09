using SnowyRiver.Accounts.Domain.Shared.Entities;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class UserAssociationEntityBase : HasCreationTimeSoftDeleteEntity<Guid>, IHasUserEntity
{
    public Guid? UserId { get; set; }
    public Guid? TeamId { get; set; }
}
