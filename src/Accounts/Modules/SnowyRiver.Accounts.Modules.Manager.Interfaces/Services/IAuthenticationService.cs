using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;

namespace SnowyRiver.Accounts.Modules.Manager.Interfaces.Services;

public interface IAuthenticationService : IAuthenticationService<User, Team, Role, Permission>;
