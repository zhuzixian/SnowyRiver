using MaterialDesignThemes.Wpf;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

public class DialogViewModelBase : DialogViewModelBase<object>
{
    protected override Task<object?> ConfirmAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<object?>(null);
    }
}

public abstract class DialogViewModelBase<T> : ViewModelBase, IDialogHostAware, IDialogAware
{
    public virtual object? IdentifierName { get; set; } = null;

    public virtual bool CanConfirm => true;
    public virtual bool CanCancel => true;

    public virtual DelegateCommand ConfirmCommand => field 
        ??= new DelegateCommand(async () =>
    {
        var value = await ConfirmAsync();
        Close(new Models.DialogResult<T>(ButtonResult.OK, value));
    })
    .ObservesCanExecute(() => CanConfirm);

    public virtual DelegateCommand CancelCommand => field ??= new DelegateCommand(async () =>
    {
        await CancelAsync();
        Close(new Models.DialogResult<T>(ButtonResult.Cancel));
    })
    .ObservesCanExecute(() => CanCancel);

    protected virtual async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    protected abstract Task<T?> ConfirmAsync(CancellationToken cancellationToken = default);

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
