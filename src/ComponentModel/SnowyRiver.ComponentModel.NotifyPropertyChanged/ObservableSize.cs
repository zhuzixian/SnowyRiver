namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

public class ObservableSize : ObservableSize<int>
{
    public ObservableSize()
    {
    }
    public ObservableSize(int width, int height) : base(width, height)
    {
    }
}

public class ObservableSize<T> : NotifyPropertyChangedObject
    where T : struct
{
    public ObservableSize()
    {
    }

    public ObservableSize(T width, T height)
    {
        Width = width;
        Height = height;
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
