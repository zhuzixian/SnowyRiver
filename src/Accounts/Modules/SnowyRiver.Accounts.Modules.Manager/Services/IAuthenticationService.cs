using System.ComponentModel;
using System.Threading.Tasks;
using SnowyRiver.Accounts.Modules.Manager.Models;

namespace SnowyRiver.Accounts.Modules.Manager.Services;

public interface IAuthenticationService<out TUser, out TTeam, TRole,  TPermission> : INotifyPropertyChanged
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public Task<(bool, LoginFailedReason)> LoginAsync(string username, string password);
    public Task LogoutAsync();

    public bool IsAuthenticated { get; }

    public TUser? User { get; }

    public TTeam? SelectedTeam { get; }
}

public enum LoginFailedReason
{
    Succeed,
    NotFoundUser,
    PasswordVerificationFailed,
    UnKnow,
}
