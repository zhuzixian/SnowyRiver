using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.EntityFramework;
namespace SnowyRiver.Accounts.Manager.EntityFramework;
public class AccountsManagerDbContext(DbContextOptions options) : AccountsDbContext(options)
{
}
