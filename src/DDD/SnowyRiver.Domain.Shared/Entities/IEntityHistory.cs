namespace SnowyRiver.Domain.Shared.Entities;

public interface IEntityHistory<TEntity, TEntityId, TUser, TTeam> 
    : ITeamAuditedEntity<TUser, TTeam>
    where TEntity : IEntity<TEntityId>
{
    TEntityId? EntityId { get; set; }
    string Action { get; set; } // Created/Updated/Deleted
    TEntity? SnapShot { get; set; }
}
