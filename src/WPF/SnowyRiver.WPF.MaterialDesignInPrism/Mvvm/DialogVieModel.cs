using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class DialogVieModel : BindableBase, IDialogHostAware
{
    public DialogVieModel()
    {
        ConfirmCommand = new DelegateCommand(Save);
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

    public virtual void Save()
    {
        Close(new DialogResult(ButtonResult.OK));
    }

    void Close(DialogResult result)
    {
        DialogHost.Close(IdentifierName, result);
    }

    public virtual Task OnDialogOpenedAsync(IDialogParameters parameters)
    {
        return Task.FromResult(true);
    }
}
