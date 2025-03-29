using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Manager.EntityFramework;

namespace SnowyRiver.Accounts.Manager.ViewModels;
public class DbMigratorViewModel(
    AccountsManagerDbContext dbContext,
    IRegionManager regionManager) : WPF.Modules.Splash.ViewModels.DbMigratorViewModel(regionManager)
{
    protected override async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
