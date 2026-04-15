namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasLastModifierUser<TUser>
{
    TUser? LastModifierUser { get; set; }
}
