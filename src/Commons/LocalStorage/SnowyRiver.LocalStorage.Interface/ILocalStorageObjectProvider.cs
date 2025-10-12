using System.ComponentModel;

namespace SnowyRiver.LocalStorage.Interface;
public interface ILocalStorageObjectProvider<T>:INotifyPropertyChanged
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task RefreshAsync(CancellationToken cancellationToken = default);

    public T? Object { get; set; }
}
