using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Core;
using SnowyRiver.Accounts.Modules.Manager;
using SnowyRiver.Accounts.Modules.Manager.ViewModels;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.Accounts.Manager.ViewModels;
public class MainWindowViewModel(IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    private DelegateCommand _handleLoadedCommand;
    public DelegateCommand HandleLoadedCommand
        => _handleLoadedCommand ??= new DelegateCommand(OnHandleLoaded);

    private void OnHandleLoaded()
    {
        var navigationParameters = new NavigationParameters
        {
            { nameof(MainViewModel.TeamsEnable), false },
            { nameof(MainViewModel.PermissionsEnable), false }
        };
        RegionManager.RequestNavigate(AccountsRegionNames.AccountsManagerViewRegion, Modules.Manager.ViewNames.MainView, navigationParameters);
    }
}
