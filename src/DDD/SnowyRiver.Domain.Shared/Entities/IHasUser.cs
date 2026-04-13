namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasUser<TUser>:IHasUserId 
{
    TUser? User { get; set; } 
}
