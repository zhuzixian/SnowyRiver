using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;

/// <summary>
/// 工具栏按钮控件，包含图标和文本。
/// </summary>
public class ToolBarIcoTextButton : Button
{
    static ToolBarIcoTextButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolBarIcoTextButton),
            new FrameworkPropertyMetadata(typeof(ToolBarIcoTextButton)));
    }

    #region Kind

    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind),
        typeof(PackIconKind),
        typeof(ToolBarIcoTextButton),
        new FrameworkPropertyMetadata(PackIconKind.None));

    /// <summary>获取或设置 Material Design 图标类型。</summary>
    public PackIconKind Kind
    {
        get => (PackIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    #endregion

    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(ToolBarIcoTextButton),
        new FrameworkPropertyMetadata(string.Empty));

    /// <summary>获取或设置按钮文本。</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion

    #region IconSize

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize),
        typeof(double),
        typeof(ToolBarIcoTextButton),
        new FrameworkPropertyMetadata(16.0));

    /// <summary>获取或设置图标大小（默认 16）。</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    #endregion

    #region IconMargin

    public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register(
        nameof(IconMargin),
        typeof(Thickness),
        typeof(ToolBarIcoTextButton),
        new FrameworkPropertyMetadata(new Thickness(0, 0, 6, 0)));

    /// <summary>获取或设置图标间距（默认右侧 6）。</summary>
    public Thickness IconMargin
    {
        get => (Thickness)GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

    #endregion
}