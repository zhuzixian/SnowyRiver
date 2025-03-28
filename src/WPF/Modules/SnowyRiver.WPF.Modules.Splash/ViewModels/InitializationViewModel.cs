using System;
using System.Threading.Tasks;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;

public class InitializationViewModel(IRegionManager regionManager): SplashContentViewModel(regionManager)
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
        RegionManager.RequestNavigate(RegionNames.InitializationViewProductInfosRegion, ViewNames.ProductInfoView);
        await InitializeAsync();
        if (navigationContext.Parameters.TryGetValue<Action>(nameof(SplashViewModel.RequestClose), out var requestClose))
        {
            requestClose.Invoke();
        }
    }

    protected virtual async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }
}