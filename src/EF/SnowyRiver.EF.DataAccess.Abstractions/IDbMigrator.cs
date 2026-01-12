namespace SnowyRiver.EF.DataAccess.Abstractions;
public interface IDbMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken = default);
}
