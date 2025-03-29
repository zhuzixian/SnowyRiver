using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
using System.Collections.ObjectModel;

namespace SnowyRiver.WPF.MaterialDesignInPrism.ViewModels;
public class DialogViewModel(IRegionManager regionManager): RegionDialogViewModelBase(regionManager)
{
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.TryGetValue<string>(nameof(Title), out var title))
        {
            Title = title;
        }

        if (navigationContext.Parameters.TryGetValue<string>(nameof(Message), out var message))
        {
            Message = message;
        }

        if (navigationContext.Parameters.TryGetValue<string[]>(nameof(Buttons), out var buttons))
        {
            Buttons = new ObservableCollection<string>(buttons);
        }

        base.OnNavigatedTo(navigationContext);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters.TryGetValue<string>(nameof(Title), out var title))
        {
            Title = title;
        }

        if (parameters.TryGetValue<string>(nameof(Message), out var message))
        {
            Message = message;
        }

        if (parameters.TryGetValue <string[]>(nameof(Buttons), out var buttons))
        {
            Buttons = new ObservableCollection<string>(buttons);
        }

        base.OnDialogOpened(parameters);
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private ObservableCollection<string> _buttons=[];
    public ObservableCollection<string> Buttons
    {
        get => _buttons;
        set => SetProperty(ref _buttons, value);
    }
}
