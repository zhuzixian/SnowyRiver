using SnowyRiver.Commons.Abstractions;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

public class ValueProvider<T> : NotifyPropertyChangedObject, IValueProvider<T>
{
    public ValueProvider()
    {
    }

    public ValueProvider(T? t)
    {
        Value = t;
    }

    public T? Value
    {
        get; 
        set => Set(ref field, value);
    }
}
