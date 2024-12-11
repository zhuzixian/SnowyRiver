using System.Collections.ObjectModel;
using SnowyRiver.Accounts.Modules.Manager.Views;

namespace SnowyRiver.Accounts.Modules.Manager.Models;
public class User : EntityModel
{
    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _passwordSalt = string.Empty;
    public string PasswordSalt
    {
        get => _passwordSalt;
        set => SetProperty(ref _passwordSalt, value);
    }

    private long _userId;
    public long UserId
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
