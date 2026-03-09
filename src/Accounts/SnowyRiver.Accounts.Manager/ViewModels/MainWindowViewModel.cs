using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Core;
using SnowyRiver.Accounts.Modules.Manager.ViewModels;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.Accounts.Manager.ViewModels;
public class MainWindowViewModel(IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    public DelegateCommand HandleLoadedCommand
        => field ??= new DelegateCommand(OnHandleLoaded);

    private void OnHandleLoaded()
    {
        var navigationParameters = new NavigationParameters
        {
            { nameof(MainViewModel.TeamsEnable), true },
            { nameof(MainViewModel.PermissionsEnable), true }
        };
        RegionManager.RequestNavigate(AccountsRegionNames.AccountsManagerViewRegion, Modules.Manager.ViewNames.MainView, navigationParameters);
    }
}
