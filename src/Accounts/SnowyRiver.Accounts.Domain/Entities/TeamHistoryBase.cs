namespace SnowyRiver.Accounts.Domain.Entities;

public class TeamHistoryBase<TUser, TRole, TTeam, TPermission>
    : AccountEntityHistory<TTeam, TUser, TTeam>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
}
