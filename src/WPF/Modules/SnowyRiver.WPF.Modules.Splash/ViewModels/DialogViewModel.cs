using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation.Regions;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;
public class DialogViewModel(IRegionManager regionManager) : MaterialDesignInPrism.ViewModels.DialogViewModel(regionManager)
{
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
        if (navigationContext.Parameters.TryGetValue<SplashDialogResult>(nameof(Result), out var result))
        {
            Result = result;
        }
    }

    public DelegateCommand<string> Command =>
        field ??= new DelegateCommand<string>((value) => _ = ExecuteAsync(value));

    protected virtual Task ExecuteAsync(string value, CancellationToken cancellationChangeToken = default)
    {
        if (Result != null)
        {
            Result.Value = value;
        }

        return Task.CompletedTask;
    }

    public SplashDialogResult? Result { get; protected set; }
}

public class SplashDialogResult
{
    public string? Value { get; set; }
}
