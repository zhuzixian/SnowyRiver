namespace SnowyRiver.LocalStorage.Interface;
public interface ILocalStorageProvider
{
    ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default);
    ValueTask<string?> GetItemAsync(string key, CancellationToken cancellationToken = default);
    ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default);
    ValueTask SetItemAsync(string key, string data, CancellationToken cancellationToken = default);
}
