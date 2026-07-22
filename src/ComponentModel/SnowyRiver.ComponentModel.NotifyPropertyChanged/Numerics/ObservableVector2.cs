namespace SnowyRiver.ComponentModel.NotifyPropertyChanged.Numerics;

public class ObservableVector2<T> : NotifyPropertyChangedObject
{
    public ObservableVector2()
    {
    }

    public ObservableVector2(T x, T y)
    {
        X = x;
        Y = y;
    }

    public T? X
    {
        get;
        set => SetProperty(ref field, value);
    }

    public T? Y
    {
        get;
        set => SetProperty(ref field, value);
    }
}
