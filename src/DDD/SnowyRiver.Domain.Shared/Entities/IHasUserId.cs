namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasUserId
{
    Guid? UserId { get; set; }
}
