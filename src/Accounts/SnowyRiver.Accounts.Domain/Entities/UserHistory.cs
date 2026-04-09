namespace SnowyRiver.Accounts.Domain.Entities;
public class UserHistory<TUser, TRole, TTeam, TPermission> 
    : HasUserEntityHistory<TUser, Guid, TUser, TRole, TTeam, TPermission>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
}
