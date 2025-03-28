using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation.Regions;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;
public class DialogViewModel(IRegionManager regionManager) : MaterialDesignInPrism.ViewModels.DialogViewModel(regionManager)
{
    private DelegateCommand<string>? _command;

    public DelegateCommand<string> Command =>
        _command ??= new DelegateCommand<string>((value) => _ = ExecuteAsync(value));

    protected virtual async Task ExecuteAsync(string value, CancellationToken cancellationChangeToken = default)
    {
        await Task.CompletedTask;
    }
}
