using System.ComponentModel;

namespace SnowyRiver.Accounts.Services.Interfaces;

public interface IAuthenticationService<out TUser, TTeam, TRole,  TPermission> : INotifyPropertyChanged
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public Task<(bool, LoginFailedReason)> LoginAsync(string username, string password);
    public Task LogoutAsync();
    public bool IsAuthenticated { get; }

    public TUser? User { get; }

    public TTeam? SelectedTeam { get; set; }
}

public enum LoginFailedReason
{
    Succeed,
    NotFoundUser,
    PasswordVerificationFailed,
    UnKnow,
    NoPermissions
}
