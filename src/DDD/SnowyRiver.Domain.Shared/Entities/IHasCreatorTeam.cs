namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasCreatorTeam<TTeam>
{
    TTeam? CreatorTeam { get; set; }
}
