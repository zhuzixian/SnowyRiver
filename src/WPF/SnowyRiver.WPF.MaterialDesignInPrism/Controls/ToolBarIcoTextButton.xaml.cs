using System.Windows;
using MaterialDesignThemes.Wpf;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;

/// <summary>
/// ToolBarButton.xaml 的交互逻辑
/// </summary>
public partial class ToolBarIcoTextButton
{
    public ToolBarIcoTextButton()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty KindProperty
        = DependencyProperty.Register(nameof(Kind), typeof(PackIconKind), typeof(ToolBarIcoTextButton), 
            new PropertyMetadata(default(PackIconKind)));

    public PackIconKind Kind
    {
        get => (PackIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }


    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(ToolBarIcoTextButton), new PropertyMetadata(default(string)));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

}