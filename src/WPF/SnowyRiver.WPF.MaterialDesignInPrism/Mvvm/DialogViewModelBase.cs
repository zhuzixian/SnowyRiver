using MaterialDesignThemes.Wpf;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class DialogViewModelBase : ViewModelBase, IDialogHostAware,IDialogAware
{
    public virtual object? IdentifierName { get; set; } = null;

    public virtual DelegateCommand ConfirmCommand => field ??= new DelegateCommand(async () =>
    {
        await ConfirmAsync();
        Close(new DialogResult(ButtonResult.OK));
    });

    public virtual DelegateCommand CancelCommand => field ??= new DelegateCommand(async () =>
    {
        await CancelAsync();
        Close(new DialogResult(ButtonResult.Cancel));
    });


    protected virtual async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task ConfirmAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    protected virtual void Close(IDialogResult result)
    {
        DialogHost.Close(IdentifierName, result);
    }

    public virtual bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
    }

    public virtual void OnDialogOpened(IDialogParameters parameters)
    {
    }

    protected void RaiseRequestClose(IDialogResult dialogResult)
    {
        RequestClose.Invoke(dialogResult);
    }

    public virtual string Title
    {
        get;
        protected set => SetProperty(ref field, value);
    } = string.Empty;

    public DialogCloseListener RequestClose { get; protected set; }
}
