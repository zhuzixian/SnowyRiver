namespace SnowyRiver.ComponentModel.Interface;
public interface IInitializable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
