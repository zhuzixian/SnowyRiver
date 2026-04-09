namespace SnowyRiver.Accounts.Domain.Shared.Entities;

public interface IHasUserEntity
{
    public Guid? TeamId { get; set; }
    public Guid? UserId { get; set; }
}