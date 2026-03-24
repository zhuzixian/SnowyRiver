using System.ComponentModel;
using SnowyRiver.Commons.Abstractions;

namespace SnowyRiver.LocalStorage.Interface;
public interface ILocalStorageValueProvider<T>:IValueProvider<T>, INotifyPropertyChanged
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task RefreshAsync(CancellationToken cancellationToken = default);
    public T? DefaultValue { get; set; }
}
