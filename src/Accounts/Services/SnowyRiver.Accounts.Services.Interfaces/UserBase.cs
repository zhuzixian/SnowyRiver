using System.Collections.ObjectModel;

namespace SnowyRiver.Accounts.Services.Interfaces;
public class User<TUser, TRole, TTeam, TPermission> : EntityModel
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public string Password
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string NewPassword
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string PasswordSalt
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public int UserId
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ObservableCollection<TRole> Roles
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    public ObservableCollection<TTeam> Teams
    {
        get;
        set => SetProperty(ref field, value);
    } = [];
}
