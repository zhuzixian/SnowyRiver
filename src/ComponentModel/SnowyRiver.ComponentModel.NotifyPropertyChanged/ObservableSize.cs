namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

public class ObservableSize : ObservableSize<int>
{
}

public class ObservableSize<T>:NotifyPropertyChangedObject
    where T:struct
{
    public T Width
    {
        get;
        set => Set(ref field, value);
    }

    public T Height
    {
        get;
        set => Set(ref field, value);
    }
}
