namespace SnowyRiver.Accounts.Modules.Manager.Models;
public class User : EntityModel
{
    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _passwordSalt = string.Empty;
    public string PasswordSalt
    {
        get => _passwordSalt;
        set => SetProperty(ref _passwordSalt, value);
    }

    private long _userId;
    public long UserId
    {
        get => _userId;
        set => SetProperty(ref _userId, value);
    }
}
