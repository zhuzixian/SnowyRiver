namespace SnowyRiver.Domain.Shared.Services;

public interface ICurrentUserServices
{
    public Guid? TeamId { get; }
    public Guid? UserId { get; }
}
