using System.ComponentModel;
using System.Windows;

namespace SnowyRiver.Accounts.Modules.Manager.Controls;

/// <summary>
/// InputTextBox.xaml 的交互逻辑
/// </summary>
public partial class TextInput
{
    public TextInput()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty TextWidthProperty = DependencyProperty.Register(
        nameof(TextWidth), typeof(double), typeof(TextInput), new PropertyMetadata(default(double)));

    [TypeConverter(typeof(LengthConverter))]

    public double TextWidth
    {
        get => (double)GetValue(TextWidthProperty);
        set => SetValue(TextWidthProperty, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

}