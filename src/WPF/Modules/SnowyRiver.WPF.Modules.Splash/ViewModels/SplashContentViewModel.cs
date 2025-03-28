using System;
using System.Threading.Tasks;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;

public class SplashContentViewModel(IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    protected async Task<string> ShowDialogAsync(string view, NavigationParameters parameters)
    {
        var dialogResult = new SplashDialogResult();
        var navigationParameters = new NavigationParameters
        {
            { nameof(DialogViewModel), dialogResult }
        };
        foreach (var parameter in parameters)
        {
            navigationParameters.Add(parameter.Key, parameter.Value!);
        }
        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, view, navigationParameters);
        while (string.IsNullOrEmpty(dialogResult.Value))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }

        return dialogResult.Value!;
    }

    protected async Task<string> ShowDialogAsync(string title, string message, string[] buttons)
    {
        return await ShowDialogAsync(ViewNames.DialogView,
            new NavigationParameters
            {
                { nameof(DialogViewModel.Title), title },
                { nameof(DialogViewModel.Message), message },
                { nameof(DialogViewModel.Buttons), buttons },
            });
    }
}
