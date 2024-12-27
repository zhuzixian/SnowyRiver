using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Models;
using SnowyRiver.Accounts.Modules.Manager.Services;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class LoginViewModel(
    IAuthenticationService<User, Team, Role, Permission> authenticationService,
    IRegionManager regionManager):LoginViewModel<User, Team, Role, Permission>(authenticationService, regionManager)
{
}
