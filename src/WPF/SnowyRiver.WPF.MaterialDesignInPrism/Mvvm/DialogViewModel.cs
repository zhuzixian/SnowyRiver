using MaterialDesignThemes.Wpf;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class DialogViewModel : ViewModelBase, IDialogHostAware,IDialogAware
{
    public string IdentifierName { get; set; } = string.Empty;

    private DelegateCommand? _confirmCommand;

    public DelegateCommand ConfirmCommand => _confirmCommand ??= new DelegateCommand(Confirm);

    private DelegateCommand? _cancelCommand;
    public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(Cancel);

    public virtual void Cancel()
    {
        Close(new DialogResult(ButtonResult.Cancel));
    }

    public virtual void Confirm()
    {
        Close(new DialogResult(ButtonResult.OK));
    }

    protected virtual void Close(IDialogResult result)
    {
        DialogHost.Close(IdentifierName, result);
    }

    public virtual Task OnDialogOpenedAsync(IDialogParameters parameters)
    {
        return Task.FromResult(true);
    }

    public virtual bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
    }

    public async void OnDialogOpened(IDialogParameters parameters)
    {
        await OnDialogOpenedAsync(parameters);
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
