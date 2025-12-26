using System.Threading.Tasks;
using Prism.Dialogs;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs
{
    /// <summary>
    /// 对话主机服务接口
    /// </summary>
    public interface IDialogHostService : IDialogService
    {
        bool IsOpen(object dialogIdentifier);

        void Close(object? dialogIdentifier = null, object? parameter = null);


        Task<IDialogResult?> ShowMaterialDesignDialogAsync(string name, object dialogIdentifier, IDialogParameters? parameters = null);
        Task<string?> ShowMaterialDesignDialogAsync(string name, string title, string message, string[] buttons,
            object dialogIdentifier);

        bool IsOpen()
        {
            return IsOpen(DefaultDialogIdentifier);
        }

        Task<IDialogResult?> ShowMaterialDesignDialogAsync(string name, IDialogParameters? parameters = null)
        {
            return ShowMaterialDesignDialogAsync(name, DefaultDialogIdentifier, parameters);
        }

        Task<string?> ShowMaterialDesignDialogAsync(string name, string title, string message, string[] buttons)
        {
            return ShowMaterialDesignDialogAsync(name, title, message, buttons, DefaultDialogIdentifier);
        }
        Task<string?> ShowMaterialDesignDialogAsync(string title, string message, string[] buttons)
        {
            return ShowMaterialDesignDialogAsync(title, title, message, buttons, DefaultDialogIdentifier);
        }


        private const string DefaultDialogIdentifier = "Root";
    }
}
