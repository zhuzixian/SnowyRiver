using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;

public class MainViewModel(IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
        if (navigationContext.Parameters.TryGetValue<bool>(nameof(TeamsEnable), out var teamsEnable))
        {
            TeamsEnable = teamsEnable;
        }

        RegionManager.RequestNavigate(RegionNames.UsersManagerViewRegion, ViewNames.UsersManagerView);
        RegionManager.RequestNavigate(RegionNames.TeamsManagerViewRegion, ViewNames.TeamsManagerView);
        RegionManager.RequestNavigate(RegionNames.RolesManagerViewRegion, ViewNames.RolesManagerView);
        RegionManager.RequestNavigate(RegionNames.PermissionsManagerViewRegion, ViewNames.PermissionsManagerView);
    }

    private bool _teamsEnable = true;
    public bool TeamsEnable
    {
        get => _teamsEnable;
        set => SetProperty(ref _teamsEnable, value);
    }
}
