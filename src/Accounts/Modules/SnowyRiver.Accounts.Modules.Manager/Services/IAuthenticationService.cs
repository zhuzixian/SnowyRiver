using System.ComponentModel;
using System.Threading.Tasks;
using SnowyRiver.Accounts.Modules.Manager.Models;

namespace SnowyRiver.Accounts.Modules.Manager.Services;

public interface IAuthenticationService : INotifyPropertyChanged
{
    public Task<(bool, LoginFailedReason)> LoginAsync(string username, string password);
    public Task LogoutAsync();

    public bool IsAuthenticated { get; }

    public User? User { get; }

    public Team? SelectedTeam { get; }
}

public enum LoginFailedReason
{
    Succeed,
    NotFoundUser,
    PasswordVerificationFailed,
    UnKnow,
}
