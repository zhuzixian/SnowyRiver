using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class HasUserEntityHistory<TEntity, TEntityId, TUser, TRole, TTeam, TPermission>
    : HasUserEntity<TUser, TRole, TTeam, TPermission>, IEntityHistory<TEntity, TEntityId>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TEntity : IEntity<TEntityId>
{
    public TEntityId? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public TEntity? SnapShot { get; set; }
}
