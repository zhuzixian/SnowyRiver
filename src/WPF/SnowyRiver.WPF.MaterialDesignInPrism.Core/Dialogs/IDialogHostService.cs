using System.Threading.Tasks;
using Prism.Dialogs;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs
{
    /// <summary>
    /// 对话主机服务接口
    /// </summary>
    public interface IDialogHostService : IDialogService
    {
        Task<IDialogResult> ShowMaterialDesignDialogAsync(string name, IDialogParameters parameters = null, string identifierName = "Root");
        Task<string> ShowMaterialDesignDialogAsync(string name, string title, string message, string[] buttons, string identifierName = "Root");
        Task<string> ShowMaterialDesignDialogAsync(string title, string message, string[] buttons, string identifierName = "Root");

    }
}
