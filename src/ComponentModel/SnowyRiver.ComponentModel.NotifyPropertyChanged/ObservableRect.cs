namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

public class ObservableRect : ObservableRect<int>
{
}

public class ObservableRect<T> : NotifyPropertyChangedObject
    where T : struct
{
    public T X
    {
        get;
        set => Set(ref field, value);
    }

    public T Y
    {
        get;
        set => Set(ref field, value);
    }

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
