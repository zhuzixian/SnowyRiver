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


        Task<IDialogResult?> ShowDialogAsync(string name, object dialogIdentifier, IDialogParameters? parameters = null);
        Task<string?> ShowDialogAsync(string name, string title, string message, string[] buttons,
            object dialogIdentifier);
        Task<string?> ShowDialogAsync(string title, string message, string[] buttons,
            object dialogIdentifier);

        bool IsOpen()
        {
            return IsOpen(DefaultDialogIdentifier);
        }

        Task<IDialogResult?> ShowDialogAsync(string name, IDialogParameters? parameters = null)
        {
            return ShowDialogAsync(name, DefaultDialogIdentifier, parameters);
        }

        Task<string?> ShowDialogAsync(string name, string title, string message, string[] buttons)
        {
            return ShowDialogAsync(name, title, message, buttons, DefaultDialogIdentifier);
        }


        Task<string?> ShowDialogAsync(string title, string message, string[] buttons)
        {
            return ShowDialogAsync(title, message, buttons, DefaultDialogIdentifier);
        }


        private const string DefaultDialogIdentifier = "Root";
    }
}
