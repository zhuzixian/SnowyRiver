namespace SnowyRiver.Domain.Shared.Entities;
public interface IInitializable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
