using System;
using System.Threading.Tasks;
using Prism.Navigation.Regions;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;
public class WelcomeViewModel(IRegionManager regionManager) : SplashContentViewModel(regionManager)
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        RegionManager.RequestNavigate(RegionNames.WelcomeContentRegion, ViewNames.ProductInfoView);
        await Task.Delay(TimeSpan.FromSeconds(1));
        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, ViewNames.DbMigratorView, 
            navigationContext.Parameters);
    }

    protected override string ViewName => ViewNames.WelcomeView;
}
