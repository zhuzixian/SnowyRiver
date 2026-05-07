namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;

public class ObservableRect : ObservableRect<int>
{
    public ObservableRect()
    {
    }

    public ObservableRect(int x, int y, int width, int height) : base(x, y, width, height)
    {
    }
}

public class ObservableRect<T> : ObservableSize<T>
    where T : struct
{
    public ObservableRect()
    {
        
    }

    public ObservableRect(T x, T y, T width, T height)
    {
        X = x;
        Y = y;
        Width = width; 
        Height = height;
    }

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
}
