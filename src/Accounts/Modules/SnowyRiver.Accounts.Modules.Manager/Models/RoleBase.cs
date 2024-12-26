using System.Collections.ObjectModel;

namespace SnowyRiver.Accounts.Modules.Manager.Models;
public class Role<TPermission> : EntityModel
    where TPermission : Permission
{
    private ObservableCollection<TPermission> _permissions = [];
    public ObservableCollection<TPermission> Permissions
    {
        get => _permissions;
        set => SetProperty(ref _permissions, value);
    }
}
