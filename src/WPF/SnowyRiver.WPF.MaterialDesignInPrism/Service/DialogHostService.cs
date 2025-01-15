using System.Windows;
using MaterialDesignThemes.Wpf;
using SnowyRiver.WPF.MaterialDesignInPrism.Common;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using SnowyRiver.WPF.MaterialDesignInPrism.ViewModels;
using SnowyRiver.WPF.MaterialDesignInPrism.Views;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Service;
/// <summary>
/// 对话主机服务
/// </summary>
public class DialogHostService(IContainerExtension containerExtension)
    : DialogService(containerExtension), IDialogHostService
{
    private readonly IContainerExtension _containerExtension = containerExtension;

    public async Task<IDialogResult?> ShowMaterialDesignDialogAsync(string name, IDialogParameters? parameters = null, string identifierName = "Root")
    {
        parameters ??= new DialogParameters();

        var content = _containerExtension.Resolve<object>(name);
        if (content is not FrameworkElement dialogContent)
            throw new NullReferenceException("A dialog's content must be a FrameworkElement");

        if (dialogContent is { DataContext: null } && ViewModelLocator.GetAutoWireViewModel(dialogContent) is null)
            ViewModelLocator.SetAutoWireViewModel(dialogContent, true);

        if (dialogContent.DataContext is not IDialogHostAware viewModel)
            throw new NullReferenceException("A dialog's ViewModel must implement the IDialogAware interface");

        viewModel.IdentifierName = identifierName;

        return (IDialogResult)(await DialogHost.Show(dialogContent, viewModel.IdentifierName, DialogOpenedEventHandler))!;

        void DialogOpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs)
        {
            var sessionContent = eventArgs.Session.Content;
            eventArgs.Session.UpdateContent(new ProgressDialog());
            if (viewModel is { } aware) aware.OnDialogOpened(parameters);
            eventArgs.Session.UpdateContent(sessionContent);
        }
    }

    public async Task<IDialogResult?> ShowMaterialDesignDialogAsync(string title, string message, string[] buttons, string identifierName = "Root")
    {
        return await ShowMaterialDesignDialogAsync(nameof(DialogView),
            new DialogParameters
            {
                { nameof(DialogViewModel.Title), title },
                { nameof(DialogViewModel.Message), message },
                { nameof(DialogViewModel.Buttons), buttons }
            }, identifierName);
    }
}
