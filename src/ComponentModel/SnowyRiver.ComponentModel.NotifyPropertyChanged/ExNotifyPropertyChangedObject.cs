using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using SnowyRiver.ComponentModel.Interface;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;
public class ExNotifyPropertyChangedObject : NotifyPropertyChangedObject,IExNotifyPropertyChanged
{
    protected virtual void OnPropertyChanged(
        object? oldValue = null, object? newValue = null, [CallerMemberName] string propertyName = null)
    {
        ExPropertyChanged?.Invoke(this, new ExPropertyChangedEventArgs(propertyName, oldValue, newValue));
    }

    protected override bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        var oldValue = field;
        if (base.Set(ref field, value, propertyName))
        {
            OnPropertyChanged(oldValue, value, propertyName);
            return true;
        }

        return false;
    }

    public event ExPropertyChangedEventHandler? ExPropertyChanged;
}
