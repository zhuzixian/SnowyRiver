using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using Prism.Dialogs;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Models;
using SnowyRiver.Accounts.Modules.Manager.ViewModels;
using SnowyRiver.Commons;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.Accounts.Manager.ViewModels;

public class SplashViewModel : DialogVieModel
{
    private readonly ILogger<SplashViewModel> _logger;

    public SplashViewModel(
        ILogger<SplashViewModel> logger,
        IRegionManager regionManager)
    {
        RegionManager = regionManager;
        _logger = logger;
    }

    private ProductInfo _productInfo;
    public ProductInfo ProductInfo
    {
        get => _productInfo;
        set => SetProperty(ref _productInfo, value);
    }

    private IRegionManager _regionManager;
    public IRegionManager RegionManager
    {
        get => _regionManager;
        set => SetProperty(ref _regionManager, value);
    }


    public override async Task OnDialogOpenedAsync(IDialogParameters parameters)
    {
        var welcomeViewDisplayMinimumTime = TimeSpan.FromSeconds(2);
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        try
        {
            ProductInfo = ReflectionHelper.GetProductInfo();
            await InitializeAsync(CancellationToken.None);

            var loginParameters = new NavigationParameters
            {
                {nameof(LoginViewModel<User, Role, Team>.NextAction), () => RaiseRequestClose(new DialogResult(ButtonResult.OK))}
            };
            RegionManager.RequestNavigate(RegionNames.SplashViewContentRegion, Modules.Manager.ViewNames.LoginView, loginParameters);
        }
        catch (Exception e)
        {
            //
        }

        while (stopWatch.Elapsed < welcomeViewDisplayMinimumTime)
        {
            var delayTime = welcomeViewDisplayMinimumTime - stopWatch.Elapsed > TimeSpan.Zero
                ? welcomeViewDisplayMinimumTime - stopWatch.Elapsed
                : TimeSpan.Zero;
            await Task.Delay(delayTime);
        }

        /*        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, ViewNames.LoginView);
                Device.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(Device.IsLoggedIn))
                    {
                        if (Device.IsLoggedIn)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            RaiseRequestClose(new DialogResult(ButtonResult.OK)));
                        }
                    }
                };
        */
        // RaiseRequestClose(new DialogResult(ButtonResult.OK));
    }


    protected virtual async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.CompletedTask;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "InitializeAsync exception");
            MessageBox.Show(e.Message);
            Environment.Exit(0);
        }
    }
}