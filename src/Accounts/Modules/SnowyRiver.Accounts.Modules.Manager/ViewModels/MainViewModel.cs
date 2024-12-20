using Prism.Navigation;
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
        if (navigationContext.Parameters.TryGetValue<bool>(nameof(PermissionsEnable), out var permissionsEnable))
        {
            PermissionsEnable = permissionsEnable;
        }

        var navigationParameters = new NavigationParameters
        {
            { nameof(teamsEnable), teamsEnable }
        };
        RegionManager.RequestNavigate(RegionNames.UsersManagerViewRegion, ViewNames.UsersManagerView, navigationParameters);
        RegionManager.RequestNavigate(RegionNames.RolesManagerViewRegion, ViewNames.RolesManagerView);
        if (TeamsEnable)
        {
            RegionManager.RequestNavigate(RegionNames.TeamsManagerViewRegion, ViewNames.TeamsManagerView);
        }
        if (permissionsEnable)
        {
            RegionManager.RequestNavigate(RegionNames.PermissionsManagerViewRegion, ViewNames.PermissionsManagerView);
        }
    }

    private bool _teamsEnable = true;
    public bool TeamsEnable
    {
        get => _teamsEnable;
        set => SetProperty(ref _teamsEnable, value);
    }

    private bool _permissionsEnable = true;
    public bool PermissionsEnable
    {
        get => _permissionsEnable;
        set => SetProperty(ref _permissionsEnable, value);
    }
}
