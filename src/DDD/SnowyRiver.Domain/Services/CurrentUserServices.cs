using SnowyRiver.Domain.Shared.Services;

namespace SnowyRiver.Domain.Services;

public class CurrentUserServices : ICurrentUserServices
{
    public virtual Guid? TeamId => null;
    public virtual Guid? UserId => null;
}
