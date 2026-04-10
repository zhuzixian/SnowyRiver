namespace SnowyRiver.Domain.Shared.Entities;

public interface ITeamAuditedEntity<T> : IAuditedEntity<T>,IHasUser,IHasTeam
{
}
