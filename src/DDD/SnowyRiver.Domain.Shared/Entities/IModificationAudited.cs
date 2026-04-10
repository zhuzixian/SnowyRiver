namespace SnowyRiver.Domain.Shared.Entities;

public interface IModificationAudited: IHasModificationTime
{
    long? LastModifierUserId { get; set; }
}
