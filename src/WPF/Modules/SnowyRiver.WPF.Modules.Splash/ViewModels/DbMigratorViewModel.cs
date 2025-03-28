﻿using System;
using System.Threading.Tasks;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.ViewModels;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;
public class DbMigratorViewModel(IRegionManager regionManager) : SplashContentViewModel(regionManager)
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
        RegionManager.RequestNavigate(RegionNames.InitializationViewProductInfosRegion, ViewNames.ProductInfoView);
        try
        {
            await MigrateAsync();
            RegionManager.RequestNavigate(RegionNames.SplashContentRegion,
                SnowyRiver.Accounts.Modules.Manager.ViewNames.LoginView,
                new NavigationParameters
                {
                    {
                        nameof(LoginViewModel.NextAction), () =>
                        {
                            RegionManager.RequestNavigate(RegionNames.SplashContentRegion, ViewNames.InitializationView,
                                navigationContext.Parameters);
                        }
                    }
                });
        }
        catch (TaskCanceledException)
        {

        }
    }

    protected virtual async Task MigrateAsync()
    {
        await Task.CompletedTask;
    }
}
