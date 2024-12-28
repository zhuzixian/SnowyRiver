using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SnowyRiver.Accounts.Manager.EntityFramework;

namespace SnowyRiver.Accounts.Manager;
public class AccountsManagerDbContextFactory : IDesignTimeDbContextFactory<AccountsManagerDbContext>
{
    public AccountsManagerDbContext CreateDbContext(string[] args)
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsManagerDbContext>()
            .AddAccountsManagerOptions()
            .Options;
        return new AccountsManagerDbContext(contextOptions);
    }
}

public static class AccountsManagerDbContextHelper
{
    public static DbContextOptionsBuilder AddAccountsManagerOptions(this DbContextOptionsBuilder options)
    {
        var connectionString = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build()
            .GetConnectionString("SQLite");
        options.UseSqlite(connectionString);
        return options;
    }
}
