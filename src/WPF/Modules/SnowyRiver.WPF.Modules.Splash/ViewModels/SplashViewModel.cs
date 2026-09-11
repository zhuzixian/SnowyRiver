using System;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;

public class SplashViewModel(IRegionManager regionManager): RegionViewModelBase(regionManager)
{
    // 保持与历史实现一致的导航参数键：子类（Welcome/Initialization 等）通过
    // nameof(SplashViewModel.RequestClose) 读取该键对应的“完成后继续”回调。
    // 由于 SplashView 已由 Dialog 改为普通导航视图，这里用一个常量承载字符串键，
    // 保证 nameof(RequestClose) 语义不变且不必再依赖 IDialogAware.RequestClose。
    public const string RequestClose = "RequestClose";

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, ViewNames.WelcomeView,
            new NavigationParameters
            {
                { RequestClose, CreateCompletedCallback(navigationContext) }
            });
    }

    /// <summary>
    /// 构造“完成”回调。由宿主（GelEp 主窗口）通过导航参数传入 onCompleted 时调用它；
    /// 若未传入，则退化为空操作，保证流程本身不会因缺少回调而中断。
    /// </summary>
    private static Action CreateCompletedCallback(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.TryGetValue<Action>("onCompleted", out var onCompleted))
        {
            return onCompleted;
        }

        return static () => { };
    }
}