using SnowyRiver.WPF.NotifyPropertyChangedBase.Commons;
using System.ComponentModel;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public abstract class QueryPagedViewModelBase<TRecord, TRecordFilter>(IRegionManager regionManager) : RegionDialogViewModelBase(regionManager)
    where TRecordFilter : QueryFilter, new()
{

    protected virtual void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
    }

    private DelegateCommand? _refreshCommand;

    public DelegateCommand? RefreshCommand
        => _refreshCommand ??= new DelegateCommand(() => _ = HandleRefreshCommandAsync(),
                () => !IsRefreshing)
            .ObservesProperty(() => IsRefreshing);

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    private async Task HandleRefreshCommandAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsRefreshing = true;
            await RefreshAsync(cancellationToken);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    protected virtual async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var records = await GetRecordsAsync(cancellationToken);
        Records.Items = records.Items;
        Records.Count = records.Count;
        Records.HasNextPage = records.HasNextPage;
        Records.HasPreviousPage = records.HasPreviousPage;
        Records.PageIndex = records.PageIndex;
        Records.PageSize = records.PageSize;
        Records.TotalCount = records.TotalCount;
        Records.TotalPages = records.TotalPages;
    }

    protected abstract Task<PagedObservableCollection<TRecord>> GetRecordsAsync(CancellationToken cancellation = default);


    private DelegateCommand? _navigateToFirstPageCommand;

    public DelegateCommand? NavigateToFirstPageCommand
        => _navigateToFirstPageCommand ??= new DelegateCommand(() => _ = NavigateToFirstPageAsync());

    protected async Task NavigateToFirstPageAsync(CancellationToken cancellationToken = default)
    {
        await NavigateToPageAsync(1, cancellationToken);
    }

    private DelegateCommand? _navigateToPreviousPageCommand;
    public DelegateCommand NavigateToPreviousPageCommand
        => _navigateToPreviousPageCommand ??= new DelegateCommand(() => _ = NavigateToPreviousPageAsync());

    protected async Task NavigateToPreviousPageAsync(CancellationToken cancellationToken = default)
    {
        await NavigateToPageAsync(Math.Max(1, Filter.PageIndex - 1), cancellationToken);
    }

    private DelegateCommand? _navigateToNextPageCommand;
    public DelegateCommand? NavigateToNextPageCommand
        => _navigateToNextPageCommand ??= new DelegateCommand(() => _ = NavigateToNextPageAsync());

    protected async Task NavigateToNextPageAsync(CancellationToken cancellationToken = default)
    {
        await NavigateToPageAsync(Math.Min(Records.TotalPages, Filter.PageIndex + 1), cancellationToken);
    }

    private DelegateCommand? _navigateToLastPageCommand;
    public DelegateCommand? NavigateToLastPageCommand
        => _navigateToLastPageCommand ??= new DelegateCommand(() => _ = NavigateToLastPageAsync());

    protected async Task NavigateToLastPageAsync(CancellationToken cancellationToken = default)
    {
        await NavigateToPageAsync(Records.TotalPages, cancellationToken);
    }

    protected virtual Task NavigateToPageAsync(int pageIndex, CancellationToken cancellationToken = default)
    {
        Filter.PageIndex = pageIndex;
        return Task.CompletedTask;
    }

    private PagedObservableCollection<TRecord> _records = new();
    public PagedObservableCollection<TRecord> Records
    {
        get => _records;
        set => SetProperty(ref _records, value);
    }

    private TRecordFilter _filter = new();
    public TRecordFilter Filter
    {
        get => _filter;
        set
        {
            var oldValue = Filter;
            if (SetProperty(ref _filter, value))
            {
                Filter.PropertyChanged += FilterOnPropertyChanged;
                oldValue.PropertyChanged -= FilterOnPropertyChanged;
            }
        }
    }
}
