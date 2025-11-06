using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class LoginViewModel(
    IAuthenticationService authenticationService,
    IRegionManager regionManager):LoginViewModel<User, Team, Role, Permission>(authenticationService, regionManager)
{
}
