using System.Windows;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;
public partial class TextFieldEditor
{
    public TextFieldEditor()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(TextFieldEditor), new PropertyMetadata(default(string)));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(TextFieldEditor), new PropertyMetadata(default(string)));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
        nameof(Unit), typeof(string), typeof(TextFieldEditor), new PropertyMetadata(default(string)));

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly DependencyProperty HeaderWidthProperty = DependencyProperty.Register(
        nameof(HeaderWidth), typeof(double), typeof(TextFieldEditor), new PropertyMetadata(0d));


    public double HeaderWidth
    {
        get => (double)GetValue(HeaderWidthProperty);
        set => SetValue(HeaderWidthProperty, value);
    }

    public static readonly DependencyProperty UnitWidthProperty = DependencyProperty.Register(
        nameof(UnitWidth), typeof(double), typeof(TextFieldEditor), new PropertyMetadata(0d));


    public double UnitWidth
    {
        get => (double)GetValue(UnitWidthProperty);
        set => SetValue(UnitWidthProperty, value);
    }
}
