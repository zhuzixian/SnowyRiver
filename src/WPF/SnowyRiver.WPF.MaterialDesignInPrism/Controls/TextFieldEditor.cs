using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;

/// <summary>
/// 可编辑的"标签-值-单位"三栏字段控件，支持 SharedSizeGroup 对齐。
/// </summary>
[TemplatePart(Name = PartTextBoxName, Type = typeof(TextBox))]
public class TextFieldEditor : FieldEditorBase
{
    private const string PartTextBoxName = "PART_TextBox";

    static TextFieldEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TextFieldEditor),
            new FrameworkPropertyMetadata(typeof(TextFieldEditor)));

        FocusableProperty.OverrideMetadata(
            typeof(TextFieldEditor),
            new FrameworkPropertyMetadata(false));
    }

    private TextBox? _innerTextBox;

    /// <summary>获取模板中的 TextBox（可用于聚焦/选择/IME 等）。</summary>
    public TextBox? InnerTextBox => _innerTextBox;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        if (_innerTextBox is not null)
        {
            _innerTextBox.KeyDown -= OnTextBoxKeyDown;
        }

        base.OnApplyTemplate();
        _innerTextBox = GetTemplateChild(PartTextBoxName) as TextBox;

        if (_innerTextBox is not null)
        {
            _innerTextBox.KeyDown += OnTextBoxKeyDown;
        }
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.OriginalSource == this && _innerTextBox is not null && !_innerTextBox.IsFocused)
        {
            _innerTextBox.Focus();
            e.Handled = true;
        }
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && EnterCommand?.CanExecute(Value) == true)
        {
            EnterCommand.Execute(Value);
            e.Handled = true;
        }
    }

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(TextFieldEditor),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
            OnValueChanged));

    /// <summary>获取或设置编辑的值（双向绑定）。</summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TextFieldEditor)d).OnValueChanged((string)e.OldValue, (string)e.NewValue);
    }

    /// <summary>值变更时调用（可供派生类覆盖以执行验证/格式化）。</summary>
    protected virtual void OnValueChanged(string oldValue, string newValue)
    {
        // 派生类可在此执行格式化或触发事件
    }

    public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
        nameof(MaxLength),
        typeof(int),
        typeof(TextFieldEditor),
        new FrameworkPropertyMetadata(0));

    /// <summary>获取或设置值的最大长度（0 表示无限制）。</summary>
    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
        nameof(TextAlignment),
        typeof(TextAlignment),
        typeof(TextFieldEditor),
        new FrameworkPropertyMetadata(TextAlignment.Left));

    /// <summary>获取或设置值文本的对齐方式。</summary>
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder),
        typeof(string),
        typeof(TextFieldEditor),
        new FrameworkPropertyMetadata(string.Empty));

    /// <summary>获取或设置占位符提示文本。</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    #endregion

    #region Commands

    public static readonly DependencyProperty EnterCommandProperty = DependencyProperty.Register(
        nameof(EnterCommand),
        typeof(ICommand),
        typeof(TextFieldEditor),
        new FrameworkPropertyMetadata(null));

    /// <summary>获取或设置按 Enter 键时执行的命令（CommandParameter 为当前 Value）。</summary>
    public ICommand? EnterCommand
    {
        get => (ICommand?)GetValue(EnterCommandProperty);
        set => SetValue(EnterCommandProperty, value);
    }

    #endregion

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
        nameof(TextWrapping), typeof(TextWrapping), typeof(TextFieldEditor), new PropertyMetadata(TextWrapping.NoWrap));

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public static readonly DependencyProperty MinLinesProperty = DependencyProperty.Register(
        nameof(MinLines), typeof(int), typeof(TextFieldEditor), new PropertyMetadata(1));

    public int MinLines
    {
        get => (int)GetValue(MinLinesProperty);
        set => SetValue(MinLinesProperty, value);
    }

    public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(
        nameof(MaxLines), typeof(int), typeof(TextFieldEditor), new PropertyMetadata(int.MaxValue));

    public int MaxLines
    {
        get => (int)GetValue(MaxLinesProperty);
        set => SetValue(MaxLinesProperty, value);
    }

}