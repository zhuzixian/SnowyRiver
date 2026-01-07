using SnowyRiver.ComponentModel.NotifyPropertyChanged;
using System.ComponentModel;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public abstract class QueryPagedViewModelBase<TRecord, TRecordFilter>: RegionDialogViewModelBase
    where TRecordFilter : QueryFilter, new()
{
    protected QueryPagedViewModelBase(IRegionManager regionManager)
        : base(regionManager)
    {
        Filter = new TRecordFilter();
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
        if (RefreshCommand != null && RefreshCommand.CanExecute())
        {
            RefreshCommand.Execute();
        }
    }

    protected virtual void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        var totalPageCount = Filter.PageSize > 0 ? (int)Math.Ceiling(Records.TotalCount / (decimal)Filter.PageSize) : 0;
        if (args.PropertyName == nameof(Filter.PageSize) && Filter.PageIndex > totalPageCount)
        {
            Filter.PageIndex = 1;
        }
        else
        {
            _ = RefreshAsync();
        }
    }

    public DelegateCommand? RefreshCommand
        => field ??= new DelegateCommand(() => _ = HandleRefreshCommandAsync(),
                () => !IsRefreshing)
            .ObservesProperty(() => IsRefreshing);

    public bool IsRefreshing
    {
        get;
        set => SetProperty(ref field, value);
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


    public DelegateCommand? NavigateToFirstPageCommand
        => field ??= new DelegateCommand(() => _ = NavigateToFirstPageAsync(),
        () => !IsRefreshing)
            .ObservesProperty(() => IsRefreshing);

    protected async Task NavigateToFirstPageAsync(CancellationToken cancellationToken = default)
    {
        await NavigateToPageAsync(1, cancellationToken);
    }

    public DelegateCommand NavigateToPreviousPageCommand
        => field ??= new DelegateCommand(() => _ = NavigateToPreviousPageAsync(),
            () => !IsRefreshing)
    .ObservesProperty(() => IsRefreshing);

    protected async Task NavigateToPreviousPageAsync(CancellationToken cancellationToken = default)
    {
        await NavigateToPageAsync(Math.Max(1, Filter.PageIndex - 1), cancellationToken);
    }

    public DelegateCommand? NavigateToNextPageCommand
        => field ??= new DelegateCommand(() => _ = NavigateToNextPageAsync(),
                () => !IsRefreshing)
            .ObservesProperty(() => IsRefreshing);

    protected async Task NavigateToNextPageAsync(CancellationToken cancellationToken = default)
    {
        await NavigateToPageAsync(Math.Min(Records.TotalPages, Filter.PageIndex + 1), cancellationToken);
    }

    public DelegateCommand? NavigateToLastPageCommand
        => field ??= new DelegateCommand(() => _ = NavigateToLastPageAsync(),
                () => !IsRefreshing)
            .ObservesProperty(() => IsRefreshing);

    protected async Task NavigateToLastPageAsync(CancellationToken cancellationToken = default)
    {
        await NavigateToPageAsync(Records.TotalPages, cancellationToken);
    }

    protected virtual Task NavigateToPageAsync(int pageIndex, CancellationToken cancellationToken = default)
    {
        Filter.PageIndex = pageIndex;
        return Task.CompletedTask;
    }

    public DelegateCommand SaveRecordsAsCommand
        => field ??= new DelegateCommand(() => _ = HandleSaveRecordsAsAsync(),
                () => !ISavingRecordsAs && Records.Items.Any())
            .ObservesProperty(() => ISavingRecordsAs);

    private async Task HandleSaveRecordsAsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            ISavingRecordsAs = true;
            await SaveRecordsAsAsync(cancellationToken);
        }
        finally
        {
            ISavingRecordsAs = false;
        }
    }

    protected virtual async Task SaveRecordsAsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    public bool ISavingRecordsAs
    {
        get;
        set => SetProperty(ref field, value);
    }

    public PagedObservableCollection<TRecord> Records
    {
        get;
        set => SetProperty(ref field, value);
    } = new();

    public TRecordFilter Filter
    {
        get;
        set
        {
            var oldValue = Filter;
            if (SetProperty(ref field, value))
            {
                Filter.PropertyChanged += FilterOnPropertyChanged;
                oldValue.PropertyChanged -= FilterOnPropertyChanged;
            }
        }
    }
}
