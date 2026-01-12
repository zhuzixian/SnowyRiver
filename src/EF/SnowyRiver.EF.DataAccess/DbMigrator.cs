using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.EF.DataAccess;

public class DbMigrator(IUnitOfWorkFactory unitOfWorkFactory): IDbMigrator
{
    public virtual async Task MigrateAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        using var unitOfWork = unitOfWorkFactory.Create();
        var dbContext = unitOfWork.DbContext;
        await PrepareForMigrateAsync(unitOfWork, cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await SeedDataAsync(unitOfWork, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    protected virtual async Task PrepareForMigrateAsync(IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task SeedDataAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
