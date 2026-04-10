namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasUser
{
    Guid? UserId { get; set; }
}