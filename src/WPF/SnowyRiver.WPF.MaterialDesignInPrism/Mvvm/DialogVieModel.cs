using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Services.Dialogs;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class DialogVieModel : ViewModelBase, IDialogHostAware,IDialogAware
{
    public DialogVieModel()
    {
        ConfirmCommand = new DelegateCommand(Confirm);
        CancelCommand = new DelegateCommand(Cancel);
    }
    public string IdentifierName { get; set; }

    private DelegateCommand _confirmCommand;

    public DelegateCommand ConfirmCommand
    {
        get => _confirmCommand;
        protected set => SetProperty(ref _confirmCommand, value);
    }

    private DelegateCommand _cancelCommand;
    public DelegateCommand CancelCommand
    {
        get => _cancelCommand;
        set => SetProperty(ref _cancelCommand, value);
    }

    public virtual void Cancel()
    {
        Close(new DialogResult(ButtonResult.Cancel));
    }

    public virtual void Confirm()
    {
        Close(new DialogResult(ButtonResult.OK));
    }

    protected void Close(DialogResult result)
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
        RequestClose?.Invoke(dialogResult);
    }

    private string _title = string.Empty;
    public virtual string Title
    {
        get => _title;
        protected set => SetProperty(ref _title, value);
    }

    public event Action<IDialogResult>? RequestClose;
}
