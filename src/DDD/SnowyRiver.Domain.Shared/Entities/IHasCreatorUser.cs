namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasCreatorUser<TUser>
{
    TUser? CreatorUser { get; set; }
}
