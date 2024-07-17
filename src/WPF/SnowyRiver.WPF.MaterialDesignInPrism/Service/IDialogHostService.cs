using Prism.Services.Dialogs;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Service
{
    /// <summary>
    /// 对话主机服务接口
    /// </summary>
    public interface IDialogHostService : IDialogService
    {
        Task<IDialogResult?> ShowDialogAsync(string name, IDialogParameters parameters = null, string IdentifierName = "Root");
    }
}
