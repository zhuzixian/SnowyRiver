using System.Windows;

namespace SnowyRiver.WPF.Proxies;
public class DataBindingProxy:Freezable
{
    protected override Freezable CreateInstanceCore()
    {
        return new DataBindingProxy();
    }

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(object), typeof(DataBindingProxy), new UIPropertyMetadata(null));
}
