namespace SnowyRiver.Accounts.Services.Interfaces;

public interface ICurrentUserServices<out TUser, out TTeam, TRole, TPermission> 
    : SnowyRiver.Domain.Shared.Services.ICurrentUserServices
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    TUser? User { get; }
    TTeam? Team { get; }
}
