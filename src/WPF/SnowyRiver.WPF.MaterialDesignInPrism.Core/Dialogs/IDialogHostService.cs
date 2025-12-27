using System.Threading.Tasks;
using Prism.Dialogs;
using SnowyRiver.Notices;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;

/// <summary>
/// 对话主机服务接口
/// </summary>
public interface IDialogHostService : IDialogService, INotifier
{
    Task<IDialogResult?> ShowDialogAsync(string name, object dialogIdentifier, IDialogParameters? parameters = null);

    Task<IDialogResult?> ShowDialogAsync(string name, IDialogParameters? parameters = null)
    {
        return ShowDialogAsync(name, DefaultIdentifier, parameters);
    }
}
