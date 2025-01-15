using Prism.Dialogs;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;

public class SplashViewModel(IRegionManager regionManager): DialogViewModelBase
{
    private IRegionManager _regionManager = regionManager;
    public IRegionManager RegionManager
    {
        get => _regionManager;
        set => SetProperty(ref _regionManager, value);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, ViewNames.WelcomeView,
            new NavigationParameters
            {
                { nameof(RequestClose), () => RaiseRequestClose(new DialogResult(ButtonResult.OK)) }
            });
    }
}