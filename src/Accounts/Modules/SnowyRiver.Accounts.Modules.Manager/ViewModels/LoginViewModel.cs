using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Services;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class LoginViewModel(
    IAuthenticationService authenticationService,
    IRegionManager regionManager):LoginViewModel<User, Team, Role, Permission>(authenticationService, regionManager)
{
}
