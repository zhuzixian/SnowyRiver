namespace SnowyRiver.Accounts.Domain.Entities;

public class HasUserEntity<TUser, TRole, TTeam, TPermission> : UserAssociationEntityBase
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public TUser? User { get; set; }
    public TTeam? Team { get; set; }
}
