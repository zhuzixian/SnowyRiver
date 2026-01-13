using SnowyRiver.ComponentModel.NotifyPropertyChanged;

namespace SnowyRiver.Accounts.Modules.Manager.Models;
public class Login : NotifyPropertyChangedObject
{
    public string UserName
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string Password
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;
}
