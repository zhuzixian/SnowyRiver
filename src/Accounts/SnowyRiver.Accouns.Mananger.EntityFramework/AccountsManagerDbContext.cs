using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Accounts.EntityFramework;
namespace SnowyRiver.Accounts.Manager.EntityFramework;
public class AccountsManagerDbContext(DbContextOptions options) 
    : AccountsDbContextBase<User, Role, Team, Permission, UserHistory, RoleHistory, TeamHistory, PermissionHistory>(options)
{
}
