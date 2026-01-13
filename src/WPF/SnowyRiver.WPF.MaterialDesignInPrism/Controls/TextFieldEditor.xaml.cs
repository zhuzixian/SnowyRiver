using System.Windows;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;
public partial class TextFieldEditor
{
    public TextFieldEditor()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(TextFieldEditor), new PropertyMetadata(default(string)));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}
