using Microsoft.EntityFrameworkCore;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.EF.DataAccess;

public class DbMigrator(IUnitOfWorkFactory unitOfWorkFactory): IDbMigrator
{
    public virtual async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        using var unitOfWork = unitOfWorkFactory.Create();
        var dbContext = unitOfWork.DbContext;
        await dbContext.Database.MigrateAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
