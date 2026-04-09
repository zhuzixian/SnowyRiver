using SnowyRiver.Accounts.Domain.Shared.Entities;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;

public class HasUserEntity<TUser, TRole, TTeam, TPermission> : HasCreationTimeSoftDeleteEntity<Guid>, IHasUserEntity
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public Guid? UserId { get; set; }
    public Guid? TeamId { get; set; }

    public TUser? User { get; set; }
    public TTeam? Team { get; set; }
}
