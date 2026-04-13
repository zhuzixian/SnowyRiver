namespace SnowyRiver.Domain.Shared.Entities;

public interface IAudited<TUser, TTeam>
    : ICreationAudited, IModificationAudited
{
    TUser? CreatorUser { get; set; }
    TTeam? CreatorTeam { get; set; }
    TUser? LastModifierUser { get; set; }
    TTeam? LastModifierTeam { get; set; }
}
