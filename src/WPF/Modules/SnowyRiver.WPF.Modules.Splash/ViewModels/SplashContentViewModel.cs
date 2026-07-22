using System;
using System.Threading.Tasks;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;

public abstract class SplashContentViewModel(IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    private INavigationParameters _navigationToParameters = new NavigationParameters();

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _navigationToParameters = navigationContext.Parameters;
        base.OnNavigatedTo(navigationContext);
    }


    /// <summary>重新导航到当前视图，并抛出 TaskCanceledException 中断当前操作流程</summary>
    protected virtual void TryAgain()
    {
        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, ViewName, _navigationToParameters);
        throw new TaskCanceledException();
    }


    /// <summary>
    /// 在 SplashContentRegion 中显示一个对话框视图，并异步等待用户响应
    /// </summary>
    /// <param name="view">要导航到的对话框视图名称</param>
    /// <param name="parameters">可选的导航参数</param>
    /// <returns>用户选择的对话框结果值</returns>
    protected virtual async Task<string> ShowDialogAsync(string view, NavigationParameters? parameters = null)
    {
        var dialogResult = new SplashDialogResult();
        var navigationParameters = new NavigationParameters
        {
            { nameof(DialogViewModel.Result), dialogResult }
        };
        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                navigationParameters.Add(parameter.Key, parameter.Value!);
            }
        }
        RegionManager.RequestNavigate(RegionNames.SplashContentRegion, view, navigationParameters);
        while (string.IsNullOrEmpty(dialogResult.Value))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }

        return dialogResult.Value!;
    }


    /// <summary>
    /// 显示带标题、消息和按钮的简单对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="message">对话框消息内容</param>
    /// <param name="buttons">按钮文本数组</param>
    /// <returns>用户点击的按钮文本</returns>
    protected virtual async Task<string> ShowDialogAsync(string title, string message, string[] buttons)
    {
        return await ShowDialogAsync(ViewNames.DialogView,
            new NavigationParameters
            {
                { nameof(DialogViewModel.Title), title },
                { nameof(DialogViewModel.Message), message },
                { nameof(DialogViewModel.Buttons), buttons },
            });
    }

    private double _progressValue;
    private double _progressMaximum = 100d;
    private bool _isProgressIndeterminate = true;
    private string _progressMessage = "正在加载...";

    /// <summary>进度条当前值（确定模式下使用，范围 0 ~ ProgressMaximum）</summary>
    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    /// <summary>进度条最大值，默认 100（配合百分比语义）</summary>
    public double ProgressMaximum
    {
        get => _progressMaximum;
        set => SetProperty(ref _progressMaximum, value);
    }

    /// <summary>是否为不确定模式。默认 true，向后兼容现有子类行为</summary>
    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        set => SetProperty(ref _isProgressIndeterminate, value);
    }

    /// <summary>进度提示文字，默认 "正在加载..."</summary>
    public string ProgressMessage
    {
        get => _progressMessage;
        set => SetProperty(ref _progressMessage, value);
    }


    /// <summary>当前视图在 Prism 导航中的唯一名称，子类必须覆写</summary>
    protected abstract string ViewName { get; }
}
