namespace SnowyRiver.Serializer.Interfaces;
public interface IJsonSerializer
{
    public Task<string> SerializeAsync<T>(T data, CancellationToken cancellationToken = default);
    public Task<T?> DeserializeAsync<T>(string? data, CancellationToken cancellationToken = default);
}
