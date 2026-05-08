using System.Windows;
using System.Windows.Controls;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;

/// <summary>
/// 密码字段编辑器，支持"标签-密码框-单位"三栏布局。
/// </summary>
[TemplatePart(Name = PartPasswordBoxName, Type = typeof(PasswordBox))]
public class PasswordFieldEditor : FieldEditorBase
{
    private const string PartPasswordBoxName = "PART_PasswordBox";

    static PasswordFieldEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PasswordFieldEditor),
            new FrameworkPropertyMetadata(typeof(PasswordFieldEditor)));

        FocusableProperty.OverrideMetadata(
            typeof(PasswordFieldEditor),
            new FrameworkPropertyMetadata(false));
    }

    private PasswordBox? _innerPasswordBox;

    /// <summary>获取模板中的 PasswordBox（可用于聚焦等）。</summary>
    public PasswordBox? InnerPasswordBox => _innerPasswordBox;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _innerPasswordBox = GetTemplateChild(PartPasswordBoxName) as PasswordBox;

        if (_innerPasswordBox is not null)
        {
            _innerPasswordBox.PasswordChanged += OnPasswordChanged;
        }
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.OriginalSource == this && _innerPasswordBox is not null && !_innerPasswordBox.IsFocused)
        {
            _innerPasswordBox.Focus();
            e.Handled = true;
        }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_innerPasswordBox is not null)
        {
            Value = _innerPasswordBox.Password;
        }
    }

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(string),
        typeof(PasswordFieldEditor),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnValueChanged));

    /// <summary>获取或设置密码值（双向绑定）。</summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (PasswordFieldEditor)d;
        if (editor._innerPasswordBox is not null && editor._innerPasswordBox.Password != (string)e.NewValue)
        {
            editor._innerPasswordBox.Password = (string)e.NewValue ?? string.Empty;
        }
    }

    #endregion
}
