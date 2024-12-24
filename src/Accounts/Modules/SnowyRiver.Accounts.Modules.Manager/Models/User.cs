using System.Collections.ObjectModel;

namespace SnowyRiver.Accounts.Modules.Manager.Models;
public class User : EntityModel
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

    private ObservableCollection<Role> _roles = [];
    public ObservableCollection<Role> Roles
    {
        get => _roles;
        set => SetProperty(ref _roles, value);
    }

    private ObservableCollection<Team> _teams = [];
    public ObservableCollection<Team> Teams
    {
        get => _teams;
        set => SetProperty(ref _teams, value);
    }
}
