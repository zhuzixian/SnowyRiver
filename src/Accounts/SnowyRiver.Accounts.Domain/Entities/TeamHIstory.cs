namespace SnowyRiver.Accounts.Domain.Entities;

public class TeamHistory<TUser, TRole, TTeam, TPermission> 
    : UserEntityHistory<TTeam, Guid, TUser, TRole, TTeam, TPermission>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
}
