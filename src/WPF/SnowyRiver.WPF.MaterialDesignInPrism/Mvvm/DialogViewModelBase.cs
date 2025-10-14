using MaterialDesignThemes.Wpf;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class DialogViewModelBase : ViewModelBase, IDialogHostAware,IDialogAware
{
    public virtual string? IdentifierName { get; set; } = null;

    private DelegateCommand? _confirmCommand;

    public virtual DelegateCommand ConfirmCommand => _confirmCommand ??= new DelegateCommand(() => _ = ConfirmAsync());

    private DelegateCommand? _cancelCommand;
    public virtual DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(() => _ = CancelAsync());

    protected virtual async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        Close(new DialogResult(ButtonResult.Cancel));
    }

    protected virtual async Task ConfirmAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        Close(new DialogResult(ButtonResult.OK));
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

    private string _title = string.Empty;
    public virtual string Title
    {
        get => _title;
        protected set => SetProperty(ref _title, value);
    }

    public DialogCloseListener RequestClose { get; protected set; }
}
