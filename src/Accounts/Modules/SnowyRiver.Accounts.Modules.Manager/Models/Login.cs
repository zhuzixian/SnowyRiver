using SnowyRiver.WPF.NotifyPropertyChangedBase;

namespace SnowyRiver.Accounts.Modules.Manager.Models;
public class Login : NotifyPropertyChangedObject
{
    private string _userName = string.Empty;
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
}
