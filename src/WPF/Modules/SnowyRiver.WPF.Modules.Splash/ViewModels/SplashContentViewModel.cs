using System;
using System.Threading.Tasks;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;

public class SplashContentViewModel(IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    protected async Task<string> ShowDialogAsync(string title, string message, string[] buttons)
    {
        var dialogResult = new SplashDialogResult();
        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, ViewNames.DialogView,
            new NavigationParameters
            {
                { nameof(DialogViewModel.Title), title },
                { nameof(DialogViewModel.Message), message },
                { nameof(DialogViewModel.Buttons), buttons },
                { nameof(DialogViewModel), dialogResult }
            });
        while (!string.IsNullOrEmpty(dialogResult.Value))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }

        return dialogResult.Value!;
    }
}
