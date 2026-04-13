namespace SnowyRiver.Domain.Shared.Entities;

public interface ITeamAuditedEntity<TUser, TTeam> 
    : IAudited<TUser, TTeam>, IHasTeam<TTeam>,IHasUser<TUser>
{
}
