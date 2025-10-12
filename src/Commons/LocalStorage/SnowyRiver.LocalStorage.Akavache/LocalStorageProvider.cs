using System.Reactive.Linq;
using Akavache;
using SnowyRiver.LocalStorage.Interface;

namespace SnowyRiver.LocalStorage.Akavache;
public class LocalStorageProvider : ILocalStorageProvider
{
    public async ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var allKeys = await CacheDatabase.LocalMachine.GetAllKeys();
        return allKeys.Contains(key);
    }

    public async ValueTask<string?> GetItemAsync(string key, CancellationToken cancellationToken = default)
    {
       return await CacheDatabase.LocalMachine.GetObject<string>(key)
           .Catch(Observable.Return(default(string?)));
    }

    public async ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default)
    {
        await CacheDatabase.LocalMachine.Invalidate(key);
    }

    public async ValueTask SetItemAsync(string key, string data, CancellationToken cancellationToken = default)
    {
        await CacheDatabase.LocalMachine.InsertObject(key, data);
    }
}
