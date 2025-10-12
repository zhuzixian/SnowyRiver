namespace SnowyRiver.LocalStorage.Interface;

public interface ILocalStorageService
{
    ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default);
    ValueTask<string?> GetItemAsStringAsync(string key, CancellationToken cancellationToken = default);
    ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default);
    ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default);
    ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default);
    ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = default);
}
