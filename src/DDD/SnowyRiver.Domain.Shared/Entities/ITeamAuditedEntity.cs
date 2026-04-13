namespace SnowyRiver.Domain.Shared.Entities;

public interface ITeamAuditedEntity<TId, TUser, TTeam> 
    : IAudited<TUser, TTeam>, IHasTeam<TTeam>,IHasUser<TUser>
{
}
