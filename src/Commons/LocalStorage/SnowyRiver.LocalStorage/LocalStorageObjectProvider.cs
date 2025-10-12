using SnowyRiver.ComponentModel.NotifyPropertyChanged;
using SnowyRiver.LocalStorage.Interface;

namespace SnowyRiver.LocalStorage;
public class LocalStorageObjectProvider<T>(
    ILocalStorageService localStorageService)
    :NotifyPropertyChangedObject, ILocalStorageObjectProvider<T>
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await RefreshAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await localStorageService.SetItemAsync(Key, Object, cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Object = await localStorageService.GetItemAsync<T?>(Key, cancellationToken);
    }


    private T? _object;

    public T? Object
    {
        get => _object; 
        set => Set(ref _object, value);
    }


    protected virtual string Key => string.Empty;
}
