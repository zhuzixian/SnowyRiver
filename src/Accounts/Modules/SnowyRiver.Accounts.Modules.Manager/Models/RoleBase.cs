using System.Collections.ObjectModel;

namespace SnowyRiver.Accounts.Modules.Manager.Models;
public class Role<TUser, TRole, TTeam, TPermission> : EntityModel
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    private ObservableCollection<TPermission> _permissions = [];
    public ObservableCollection<TPermission> Permissions
    {
        get => _permissions;
        set => SetProperty(ref _permissions, value);
    }

    private ObservableCollection<TUser> _users = [];
    public ObservableCollection<TUser> Users
    {
        get => _users; 
        set => Set(ref _users, value);
    }
}
