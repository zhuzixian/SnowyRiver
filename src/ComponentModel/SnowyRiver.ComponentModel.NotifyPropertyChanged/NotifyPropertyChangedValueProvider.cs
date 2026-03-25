using SnowyRiver.Commons.Abstractions;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

public class NotifyPropertyChangedValueProvider<T> : NotifyPropertyChangedObject,IValueProvider<T>
{
    public T? Value
    {
        get;
        set => Set(ref field, value);
    }
}
