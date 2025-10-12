using System.ComponentModel;

namespace SnowyRiver.LocalStorage.Interface;
public interface ILocalStorageValueProvider<T>:INotifyPropertyChanged
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task RefreshAsync(CancellationToken cancellationToken = default);

    public T? Value { get; set; }
}
