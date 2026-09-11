using System;
using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Core;
using SnowyRiver.Accounts.Modules.Manager.ViewModels;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.Accounts.Manager.ViewModels;
public class MainWindowViewModel(
    IDialogHostService dialog,
    IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    public DelegateCommand HandleLoadedCommand
        => field ??= new DelegateCommand(OnHandleLoaded);

    private void OnHandleLoaded()
    {
        // 启动流程外包在 SplashView（欢迎页 → 数据库迁移 → 登录 → 初始化）中，
        // 完成后通过 onCompleted 回调进入主界面。
        var navigationParameters = new NavigationParameters
        {
            { "onCompleted", new Action(NavigateToMainView) }
        };
        RegionManager.RequestNavigate(AccountsRegionNames.AccountsManagerViewRegion,
            WPF.Modules.Splash.ViewNames.SplashView, navigationParameters);
    }

    private void NavigateToMainView()
    {
        // 进入主界面：先恢复正常的窗口外观并最大化，再切换主界面内容，
        // 使窗口形态变化与内容渲染同步，避免“先小窗停顿再变大”的割裂感。
        if (System.Windows.Application.Current?.MainWindow is System.Windows.Window mainWindow)
        {
            if (mainWindow is MahApps.Metro.Controls.MetroWindow metroWindow)
            {
                metroWindow.ShowTitleBar = true;
                metroWindow.ShowMinButton = true;
                metroWindow.ShowMaxRestoreButton = true;
                metroWindow.ShowCloseButton = true;
            }

            mainWindow.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            mainWindow.ResizeMode = System.Windows.ResizeMode.CanResize;
            mainWindow.WindowState = System.Windows.WindowState.Maximized;
        }

        var navigationParameters = new NavigationParameters
        {
            { nameof(MainViewModel.TeamsEnable), true },
            { nameof(MainViewModel.PermissionsEnable), true }
        };
        RegionManager.RequestNavigate(AccountsRegionNames.AccountsManagerViewRegion,
            Modules.Manager.ViewNames.MainView, navigationParameters);
    }

    public DelegateCommand NavigationToLoginCommand
        => field ??= new DelegateCommand(() => _ = LoginAsync());

    private async Task LoginAsync(CancellationToken cancellationToken = default)
    {
        await dialog.ShowDialogAsync(SnowyRiver.Accounts.Modules.Manager.ViewNames.LoginView,
            "Root");
    }
}
