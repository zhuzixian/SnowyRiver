using Prism.Ioc;
using Prism.Modularity;
using SnowyRiver.Accounts.Modules.Manager.Views;
using SnowyRiver.Accounts.Manager.ViewModels;

namespace SnowyRiver.Accounts.Manager;

public class AccountsManagerAppModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<PermissionEditorView,
            PermissionEditorViewModel>(
            Modules.Manager.ViewNames.PermissionEditorView);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
