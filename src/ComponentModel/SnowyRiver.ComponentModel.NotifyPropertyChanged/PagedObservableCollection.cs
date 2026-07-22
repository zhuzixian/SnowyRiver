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

    public int? PageIndex
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int? PageSize
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int Count
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int TotalCount
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int TotalPages
    {
        get;
        set => SetProperty(ref field, value);
    }


    public bool HasPreviousPage
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool HasNextPage
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ObservableCollection<T> Items
    {
        get;
        set => SetProperty(ref field, value);
    } = [];
}
