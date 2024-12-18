using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;

public class MainViewModel(IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        regionManager.RequestNavigate(RegionNames.UsersManagerViewRegion, ViewNames.UsersManagerView);
        regionManager.RequestNavigate(RegionNames.TeamsManagerViewRegion, ViewNames.TeamsManagerView);
        regionManager.RequestNavigate(RegionNames.RolesManagerViewRegion, ViewNames.RolesManagerView);
        regionManager.RequestNavigate(RegionNames.PermissionsManagerViewRegion, ViewNames.PermissionsManagerView);
    }
}
