using SnowyRiver.LocalStorage.Interface;
using SnowyRiver.Serializer.Interfaces;

namespace SnowyRiver.LocalStorage;
public class LocalStorageService(IJsonSerializer jsonSerializer, ILocalStorageProvider storageProvider):ILocalStorageService
{
    public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var text = await storageProvider.GetItemAsync(key, cancellationToken);
        return await jsonSerializer.DeserializeAsync<T>(text, cancellationToken);
    }

    public async ValueTask<string?> GetItemAsStringAsync(string key, CancellationToken cancellationToken = default)
    {
        if (await storageProvider.ContainKeyAsync(key, cancellationToken))
        {
            return await storageProvider.GetItemAsync(key, cancellationToken);
        }

        return null;
    }

    public async ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default)
    {
        if (await storageProvider.ContainKeyAsync(key, cancellationToken))
        {
            await storageProvider.RemoveItemAsync(key, cancellationToken);
        }
    }

    public ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return storageProvider.ContainKeyAsync(key, cancellationToken);
    }

    public async ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default)
    {
        var text = await jsonSerializer.SerializeAsync(data, cancellationToken);
        await storageProvider.SetItemAsync(key, text, cancellationToken);
    }

    public async ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = default)
    {
        await storageProvider.SetItemAsync(key, data, cancellationToken);
    }
}
