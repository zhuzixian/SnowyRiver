namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

public class ObservableRange<T>:NotifyPropertyChangedObject
{
    public ObservableRange()
    {
    }

    public ObservableRange(T? min, T? max)
    {
        Min = min;
        Max = max;
    }

    public T? Max
    {
        get;
        set => SetProperty(ref field, value);
    }

    public T? Min
    {
        get;
        set => SetProperty(ref field, value);
    }
}
