using System.Collections.ObjectModel;

namespace SnowyRiver.Accounts.Services.Interfaces;
public class Permission<TUser, TRole, TTeam, TPermission> : EntityModel
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public ObservableCollection<TRole> Roles
    {
        get;
        set => Set(ref field, value);
    } = [];

    public string? Code
    {
        get;
        set => Set(ref field, value);
    }

    public string? Alias
    {
        get;
        set => SetProperty(ref field, value);
    }
}
