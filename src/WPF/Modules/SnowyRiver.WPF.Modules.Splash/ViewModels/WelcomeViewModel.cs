using Prism.Navigation.Regions;
using System;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;
public class WelcomeViewModel(IRegionManager regionManager) : SplashContentViewModel(regionManager)
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        RegionManager.RequestNavigate(RegionNames.WelcomeContentRegion, ViewNames.ProductInfoView);
        if (!string.IsNullOrWhiteSpace(NextView))
        {
            RegionManager.RequestNavigate(RegionNames.SplashContentRegion, NextView, 
                navigationContext.Parameters);
        }
        else
        {
            if (navigationContext.Parameters.TryGetValue<Action>(nameof(SplashViewModel.RequestClose), out var requestClose))
            {
                requestClose.Invoke();
            }
        }
    }

    protected virtual string? NextView => ViewNames.DbMigratorView;

    protected override string ViewName => ViewNames.WelcomeView;
}
