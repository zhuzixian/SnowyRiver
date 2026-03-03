namespace SnowyRiver.ComponentModel.NotifyPropertyChanged.Numerics;

public class ObservableVector3<T> : ObservableVector2<T>
{
    public ObservableVector3()
    {
    }

    public ObservableVector3(T x, T y, T z):base(x, y)
    {
        Z = z;
    }

    public T? Z
    {
        get;
        set => Set(ref field, value);
    }
}
