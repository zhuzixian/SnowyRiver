using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;

namespace SnowyRiver.Accounts.EntityFramework;

public class AccountsDbContext(DbContextOptions options)
    : AccountsDbContextBase<User, Role, Team, Permission, UserHistory, RoleHistory, TeamHistory, PermissionHistory>(
        options)
{

}
