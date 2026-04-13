namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasTeam<TTeam>:IHasTeamId
{
    TTeam? Team { get; set; }
}