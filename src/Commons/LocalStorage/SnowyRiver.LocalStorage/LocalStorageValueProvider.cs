﻿using SnowyRiver.ComponentModel.NotifyPropertyChanged;
using SnowyRiver.LocalStorage.Interface;

namespace SnowyRiver.LocalStorage;
public abstract class LocalStorageValueProvider<T>(
    ILocalStorageService localStorageService)
    :NotifyPropertyChangedObject, ILocalStorageValueProvider<T>
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await RefreshAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await localStorageService.SetItemAsync(Key, Value, cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Value = await localStorageService.GetItemAsync<T?>(Key, cancellationToken);
    }


    private T? _value;

    public T? Value
    {
        get => _value; 
        set => Set(ref _value, value);
    }


    protected abstract string Key { get; }
}
