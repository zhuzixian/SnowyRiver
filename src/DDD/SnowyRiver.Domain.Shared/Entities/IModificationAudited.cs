namespace SnowyRiver.Domain.Shared.Entities;

public interface IModificationAudited: IHasModificationTime
{
    Guid? LastModifierUserId { get; set; }
}
