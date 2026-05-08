using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;

/// <summary>
/// 下拉框字段编辑器，支持"标签-下拉框-单位"三栏布局。
/// </summary>
[TemplatePart(Name = PartComboBoxName, Type = typeof(ComboBox))]
public class ComboBoxFieldEditor : FieldEditorBase
{
    private const string PartComboBoxName = "PART_ComboBox";

    static ComboBoxFieldEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ComboBoxFieldEditor),
            new FrameworkPropertyMetadata(typeof(ComboBoxFieldEditor)));

        FocusableProperty.OverrideMetadata(
            typeof(ComboBoxFieldEditor),
            new FrameworkPropertyMetadata(false));
    }

    private ComboBox? _innerComboBox;

    /// <summary>获取模板中的 ComboBox（可用于聚焦等）。</summary>
    public ComboBox? InnerComboBox => _innerComboBox;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _innerComboBox = GetTemplateChild(PartComboBoxName) as ComboBox;
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.OriginalSource == this && _innerComboBox is not null && !_innerComboBox.IsFocused)
        {
            _innerComboBox.Focus();
            e.Handled = true;
        }
    }

    #region ComboBox Properties

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(ComboBoxFieldEditor),
        new FrameworkPropertyMetadata(null));

    /// <summary>获取或设置数据源。</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
        nameof(DisplayMemberPath),
        typeof(string),
        typeof(ComboBoxFieldEditor),
        new FrameworkPropertyMetadata(string.Empty));

    /// <summary>获取或设置显示成员路径。</summary>
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(object),
        typeof(ComboBoxFieldEditor),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>获取或设置选中项（双向绑定）。</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
        nameof(SelectedValue),
        typeof(object),
        typeof(ComboBoxFieldEditor),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>获取或设置选中值（双向绑定）。</summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register(
        nameof(SelectedValuePath),
        typeof(string),
        typeof(ComboBoxFieldEditor),
        new FrameworkPropertyMetadata(string.Empty));

    /// <summary>获取或设置选中值路径。</summary>
    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    #endregion
}
