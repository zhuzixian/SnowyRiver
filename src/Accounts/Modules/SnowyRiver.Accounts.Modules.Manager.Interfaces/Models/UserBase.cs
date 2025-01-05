using System.Collections.ObjectModel;

namespace SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
public class User<TUser, TRole, TTeam, TPermission> : EntityModel
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    private string _passwordSalt = string.Empty;
    public string PasswordSalt
    {
        get => _passwordSalt;
        set => SetProperty(ref _passwordSalt, value);
    }

    private int _userId;
    public int UserId
    {
        get => _userId;
        set => SetProperty(ref _userId, value);
    }

    private ObservableCollection<TRole> _roles = [];
    public ObservableCollection<TRole> Roles
    {
        get => _roles;
        set => SetProperty(ref _roles, value);
    }

    private ObservableCollection<TTeam> _teams = [];
    public ObservableCollection<TTeam> Teams
    {
        get => _teams;
        set => SetProperty(ref _teams, value);
    }
}
