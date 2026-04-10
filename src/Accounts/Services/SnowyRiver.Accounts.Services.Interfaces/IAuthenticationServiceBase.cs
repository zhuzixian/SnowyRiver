using System.ComponentModel;

namespace SnowyRiver.Accounts.Services.Interfaces;

public interface IAuthenticationService<out TUser, TTeam, TRole,  TPermission>
    : INotifyPropertyChanged, ICurrentUserServices<TUser, TTeam, TRole, TPermission>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    Task<(bool, LoginFailedReason)> LoginAsync(string username, string password,
        CancellationToken cancellationToken = default);
    Task LogoutAsync();
    bool IsAuthenticated => User != null;
    Task ChangeTeamAsync(TTeam team, CancellationToken cancellationToken = default);
}

public enum LoginFailedReason
{
    Succeed,
    NotFoundUser,
    PasswordVerificationFailed,
    UnKnow,
    NoPermissions
}
