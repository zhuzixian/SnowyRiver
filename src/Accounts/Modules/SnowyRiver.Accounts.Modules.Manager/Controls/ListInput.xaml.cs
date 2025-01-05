using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;

namespace SnowyRiver.Accounts.Modules.Manager.Controls;
/// <summary>
/// LIstSelector.xaml 的交互逻辑
/// </summary>
public partial class ListInput:INotifyPropertyChanged
{
    public ListInput()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
        nameof(HeaderHeight), typeof(double), typeof(ListInput), new PropertyMetadata(double.NaN));

    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(ListInput), new PropertyMetadata(default(string)));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(ListInput), 
        new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText), typeof(string), typeof(ListInput), new PropertyMetadata(default(string)));

    static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListInput listInput)
        {
            listInput.RefreshSearchResult();
        }
    }

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    private ObservableCollection<object> _searchResult = [];

    public ObservableCollection<object> SearchResult
    {
        get => _searchResult;
        set => Set(ref _searchResult, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshSearchResult();
    }

    public void RefreshSearchResult()
    {
        SearchResult.Clear();
        foreach (var item in ItemsSource)
        {
            if (string.IsNullOrEmpty(SearchText) || item is EntityModel entity && entity.Name.ToLower().Contains(SearchText.ToLower()))
            {
                SearchResult.Add(item);
            }
        }
    }
}
