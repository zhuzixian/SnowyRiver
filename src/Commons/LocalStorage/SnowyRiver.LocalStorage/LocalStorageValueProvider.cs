using SnowyRiver.ComponentModel.NotifyPropertyChanged;
using SnowyRiver.LocalStorage.Interface;

namespace SnowyRiver.LocalStorage;
public class LocalStorageValueProvider<T>
    :NotifyPropertyChangedObject, ILocalStorageValueProvider<T>
{
    private readonly ILocalStorageService _localStorageService;
    public LocalStorageValueProvider(
        ILocalStorageService localStorageService,
        string? key = null,
        T? defaultValue = default)
    {
        _localStorageService = localStorageService;
        if (!string.IsNullOrWhiteSpace(key))
        {
            Key = key;
        }

        if (defaultValue != null)
        {
            DefaultValue = defaultValue;
        }
    }


    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await RefreshAsync(cancellationToken);
    }

    public virtual async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _localStorageService.SetItemAsync(Key, Value, cancellationToken);
    }

    public virtual async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (await _localStorageService.ContainKeyAsync(Key, cancellationToken))
        {
            Value = await _localStorageService.GetItemAsync<T?>(Key, cancellationToken);
        }
        else
        {
            Value = DefaultValue;
        }
    }


    public T? Value
    {
        get;
        set => Set(ref field, value);
    }

    public T? DefaultValue
    {
        get; 
        set;
    }

    protected virtual string Key { get; }
}
