using System.Collections.ObjectModel;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;
public class PagedObservableCollection<T> : NotifyPropertyChangedObject
{
    public PagedObservableCollection(){}

    public PagedObservableCollection(IList<T> source, int? pageIndex, int? pageSize, int totalCount)
    {
        if (source?.Any() ?? false)
        {
            PageIndex = pageIndex ?? 1;
            PageSize = pageSize;
            Count = source.Count;
            TotalCount = totalCount;
            TotalPages = PageSize > 0 ? (int)Math.Ceiling(TotalCount / (decimal)PageSize.Value) : 0;
            Items = new ObservableCollection<T>(source);
        }
    }

    private int? _pageIndex;
    public int? PageIndex
    {
        get => _pageIndex;
        set => Set(ref _pageIndex, value);
    }

    private int? _pageSize;
    public int? PageSize
    {
        get => _pageSize;
        set => Set(ref _pageSize, value);
    }

    private int _count;

    public int Count
    {
        get => _count;
        set => Set(ref _count, value);
    }

    private int _totalCount;

    public int TotalCount
    {
        get => _totalCount;
        set => Set(ref _totalCount, value);
    }

    private int _totalPages;

    public int TotalPages
    {
        get => _totalPages;
        set => Set(ref _totalPages, value);
    }


    private bool _hasPreviousPage;

    public bool HasPreviousPage
    {
        get => _hasPreviousPage;
        set => Set(ref _hasPreviousPage, value);
    }

    private bool _hasNextPage;

    public bool HasNextPage
    {
        get => _hasPreviousPage;
        set => Set(ref _hasNextPage, value);
    }

    private ObservableCollection<T> _items = [];

    public ObservableCollection<T> Items
    {
        get => _items;
        set => Set(ref _items, value);
    }
}
