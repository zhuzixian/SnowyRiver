using System.Collections;
using System.Windows;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Controls;
public partial class ComboBoxFieldEditor
{
    public ComboBoxFieldEditor()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(IEnumerable)));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
        nameof(DisplayMemberPath), typeof(string), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(string)));

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(object)));
    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
        nameof(SelectedValue), typeof(object), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(object)));
    public object SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register(
        nameof(SelectedValuePath), typeof(string), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(string)));

    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(string)));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
        nameof(Unit), typeof(string), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(string)));

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register(
        nameof(HeaderStyle), typeof(Style), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(Style)));

    public Style HeaderStyle
    {
        get => (Style)GetValue(HeaderStyleProperty);
        set => SetValue(HeaderStyleProperty, value);
    }

    public static readonly DependencyProperty ValueStyleProperty = DependencyProperty.Register(
        nameof(ValueStyle), typeof(Style), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(Style)));

    public Style ValueStyle
    {
        get => (Style)GetValue(ValueStyleProperty);
        set => SetValue(ValueStyleProperty, value);
    }

    public static readonly DependencyProperty UnitStyleProperty = DependencyProperty.Register(
        nameof(UnitStyle), typeof(Style), typeof(ComboBoxFieldEditor), new PropertyMetadata(default(Style)));

    public Style UnitStyle
    {
        get => (Style)GetValue(UnitStyleProperty);
        set => SetValue(UnitStyleProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(ComboBoxFieldEditor), new PropertyMetadata(false));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
}
