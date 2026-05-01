using System.Windows;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;
public partial class TextFieldEditor
{
    public TextFieldEditor()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(TextFieldEditor), new FrameworkPropertyMetadata(
            null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(TextFieldEditor), new PropertyMetadata(default(string)));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
        nameof(Unit), typeof(string), typeof(TextFieldEditor), new PropertyMetadata(default(string)));

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register(
        nameof(HeaderStyle), typeof(Style), typeof(TextFieldEditor), new PropertyMetadata(default(Style)));

    public Style HeaderStyle
    {
        get => (Style)GetValue(HeaderStyleProperty);
        set => SetValue(HeaderStyleProperty, value);
    }

    public static readonly DependencyProperty ValueStyleProperty = DependencyProperty.Register(
        nameof(ValueStyle), typeof(Style), typeof(TextFieldEditor), new PropertyMetadata(default(Style)));

    public Style ValueStyle
    {
        get => (Style)GetValue(ValueStyleProperty);
        set => SetValue(ValueStyleProperty, value);
    }

    public static readonly DependencyProperty UnitStyleProperty = DependencyProperty.Register(
        nameof(UnitStyle), typeof(Style), typeof(TextFieldEditor), new PropertyMetadata(default(Style)));

    public Style UnitStyle
    {
        get => (Style)GetValue(UnitStyleProperty);
        set => SetValue(UnitStyleProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(TextFieldEditor), new PropertyMetadata(false));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty HeaderSizeGroupProperty =
        DependencyProperty.Register(nameof(HeaderSizeGroup), typeof(string), typeof(TextFieldEditor), new PropertyMetadata(null));

    public string HeaderSizeGroup
    {
        get => (string)GetValue(HeaderSizeGroupProperty);
        set => SetValue(HeaderSizeGroupProperty, value);
    }

    public static readonly DependencyProperty UnitSizeGroupProperty =
        DependencyProperty.Register(nameof(UnitSizeGroup), typeof(string), typeof(TextFieldEditor), new PropertyMetadata(null));

    public string UnitSizeGroup
    {
        get => (string)GetValue(UnitSizeGroupProperty);
        set => SetValue(UnitSizeGroupProperty, value);
    }
}
