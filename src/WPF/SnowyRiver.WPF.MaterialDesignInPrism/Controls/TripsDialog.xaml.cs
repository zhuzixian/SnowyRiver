using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;
/// <summary>
/// Interaction logic for TripsDialog
/// </summary>
public partial class TripsDialog : UserControl
{
    public TripsDialog()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty TileProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(TripsDialog), new PropertyMetadata(default(string)));

    public string Title
    {
        get => (string)GetValue(TileProperty);
        set => SetValue(TileProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(TripsDialog), new PropertyMetadata(default(string)));

    public string Message
    {
        get => (string)GetValue(TileProperty);
        set => SetValue(TileProperty, value);
    }

    public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(
        nameof(Buttons), typeof(IEnumerable), typeof(TripsDialog), new PropertyMetadata(default(IEnumerable)));

    public IEnumerable Buttons
    {
        get => (IEnumerable)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
}
