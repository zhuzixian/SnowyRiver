using System.Windows;
using System.Windows.Controls;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;

/// <summary>
/// "标签-值-单位"三栏字段控件的抽象基类，提供共性属性。
/// </summary>
public abstract class FieldEditorBase : Control
{
    #region Header

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(string),
        typeof(FieldEditorBase),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>获取或设置左侧标签文本。</summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register(
        nameof(HeaderStyle),
        typeof(Style),
        typeof(FieldEditorBase),
        new FrameworkPropertyMetadata(null));

    /// <summary>获取或设置标签的样式。</summary>
    public Style? HeaderStyle
    {
        get => (Style?)GetValue(HeaderStyleProperty);
        set => SetValue(HeaderStyleProperty, value);
    }

    public static readonly DependencyProperty HeaderSizeGroupProperty = DependencyProperty.Register(
        nameof(HeaderSizeGroup),
        typeof(string),
        typeof(FieldEditorBase),
        new FrameworkPropertyMetadata(null));

    /// <summary>获取或设置标签栏的 SharedSizeGroup 名称（用于多行对齐）。</summary>
    public string? HeaderSizeGroup
    {
        get => (string?)GetValue(HeaderSizeGroupProperty);
        set => SetValue(HeaderSizeGroupProperty, value);
    }

    public static readonly DependencyProperty HeaderVerticalAlignmentProperty = DependencyProperty.Register(
        nameof(HeaderVerticalAlignment), typeof(VerticalAlignment), typeof(FieldEditorBase), new PropertyMetadata(VerticalAlignment.Center));

    public VerticalAlignment HeaderVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(HeaderVerticalAlignmentProperty);
        set => SetValue(HeaderVerticalAlignmentProperty, value);
    }

    #endregion

    #region Value Style

    public static readonly DependencyProperty ValueStyleProperty = DependencyProperty.Register(
        nameof(ValueStyle),
        typeof(Style),
        typeof(FieldEditorBase),
        new FrameworkPropertyMetadata(null));

    /// <summary>获取或设置值编辑框的样式。</summary>
    public Style? ValueStyle
    {
        get => (Style?)GetValue(ValueStyleProperty);
        set => SetValue(ValueStyleProperty, value);
    }

    #endregion

    #region Unit

    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
        nameof(Unit),
        typeof(string),
        typeof(FieldEditorBase),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>获取或设置右侧单位文本。</summary>
    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly DependencyProperty UnitStyleProperty = DependencyProperty.Register(
        nameof(UnitStyle),
        typeof(Style),
        typeof(FieldEditorBase),
        new FrameworkPropertyMetadata(null));

    /// <summary>获取或设置单位的样式。</summary>
    public Style? UnitStyle
    {
        get => (Style?)GetValue(UnitStyleProperty);
        set => SetValue(UnitStyleProperty, value);
    }

    public static readonly DependencyProperty UnitSizeGroupProperty = DependencyProperty.Register(
        nameof(UnitSizeGroup),
        typeof(string),
        typeof(FieldEditorBase),
        new FrameworkPropertyMetadata(null));

    /// <summary>获取或设置单位栏的 SharedSizeGroup 名称（用于多行对齐）。</summary>
    public string? UnitSizeGroup
    {
        get => (string?)GetValue(UnitSizeGroupProperty);
        set => SetValue(UnitSizeGroupProperty, value);
    }

    #endregion

    #region Common Properties

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(FieldEditorBase),
        new FrameworkPropertyMetadata(false));

    /// <summary>获取或设置值是否只读。</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion
}
