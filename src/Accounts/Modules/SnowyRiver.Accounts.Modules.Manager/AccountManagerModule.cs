using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Core;
using SnowyRiver.Accounts.Modules.Manager.Views;

namespace SnowyRiver.Accounts.Modules.Manager
{
    public class AccountManagerModule(IRegionManager regionManager): IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            regionManager.RequestNavigate(AccountsRegionNames.AccountsManagerViewRegion, ViewNames.MainView);

            regionManager.RequestNavigate(RegionNames.UsersManagerViewRegion, ViewNames.UsersManagerView);
            regionManager.RequestNavigate(RegionNames.TeamsManagerViewRegion, ViewNames.TeamsManagerView);
            regionManager.RequestNavigate(RegionNames.RolesManagerViewRegion, ViewNames.RolesManagerView);
            regionManager.RequestNavigate(RegionNames.PermissionsManagerViewRegion, ViewNames.PermissionsManagerView);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainView>(ViewNames.MainView);

            containerRegistry.RegisterForNavigation<UserEditorView>(ViewNames.UserEditorView);
            containerRegistry.RegisterForNavigation<UsersManagerView>(ViewNames.UsersManagerView);
            containerRegistry.RegisterForNavigation<TeamEditorView>(ViewNames.TeamEditorView);
            containerRegistry.RegisterForNavigation<TeamsManagerView>(ViewNames.TeamsManagerView);
            containerRegistry.RegisterForNavigation<RolesManagerView>(ViewNames.RolesManagerView);
            containerRegistry.RegisterForNavigation<RoleEditorView>(ViewNames.RoleEditorView);
            containerRegistry.RegisterForNavigation<PermissionsManagerView>(ViewNames.PermissionsManagerView);
            containerRegistry.RegisterForNavigation<PermissionEditorView>(ViewNames.PermissionEditorView);
        }
    }
}