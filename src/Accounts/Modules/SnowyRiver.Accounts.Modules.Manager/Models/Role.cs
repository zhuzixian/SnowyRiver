using System.Collections.ObjectModel;

namespace SnowyRiver.Accounts.Modules.Manager.Models;
public class Role : EntityModel
{
    private ObservableCollection<Permission> _permissions = [];
    public ObservableCollection<Permission> Permissions
    {
        get => _permissions;
        set => SetProperty(ref _permissions, value);
    }
}
