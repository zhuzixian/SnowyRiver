using System.Threading.Tasks;
using Prism.Dialogs;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;

public class SplashViewModel(IRegionManager regionManager): DialogVieModel
{
    private IRegionManager _regionManager = regionManager;
    public IRegionManager RegionManager
    {
        get => _regionManager;
        set => SetProperty(ref _regionManager, value);
    }

    public override async Task OnDialogOpenedAsync(IDialogParameters parameters)
    {
        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, ViewNames.WelcomeView,
            new NavigationParameters
            {
                { nameof(RequestClose), () => RaiseRequestClose(new DialogResult(ButtonResult.OK)) }
            });
        await Task.CompletedTask;
    }
}