using System.Collections.ObjectModel;

namespace SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
public class Permission<TUser, TRole, TTeam, TPermission> : EntityModel
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    private ObservableCollection<TRole> _roles = [];
    public ObservableCollection<TRole> Roles
    {
        get => _roles;
        set => Set(ref _roles, value);
    }

    private string? _alias;
    public string? Alias
    {
        get => _alias;
        set => SetProperty(ref _alias, value);
    }
}
