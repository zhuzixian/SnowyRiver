namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class PagedViewModelBase(int pageSize, IRegionManager regionManager):RegionViewModelBase(regionManager)
{
    private DelegateCommand? _refreshCommand;
    public DelegateCommand RefreshCommand
        => _refreshCommand ??= new DelegateCommand(() => _ = RefreshCommandExecuteAsync())
            .ObservesCanExecute(() => CanRefresh);

    protected virtual bool CanRefresh => 
        !IsRefreshing && !IsNavigatingToFirstPage && !IsNavigatingToLastPage && !IsNavigatingToPreviousPage && !IsNavigatingToNextPage;

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (SetProperty(ref _isRefreshing, value))
            {
                RaisePropertyChanged(nameof(CanRefresh));
                RaisePropertyChanged(nameof(CanNavigateToPreviousPage));
                RaisePropertyChanged(nameof(CanNavigateToNextPage));
                RaisePropertyChanged(nameof(CanNavigateToFirstPage));
                RaisePropertyChanged(nameof(CanNavigateToLastPage));
            }
        }
    }

    private async Task RefreshCommandExecuteAsync()
    {
        try
        {
            IsRefreshing = true;
            await RefreshAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    protected virtual async Task RefreshAsync()
    {
        await Task.CompletedTask;
    }


    private DelegateCommand? _navigateToPreviousPageCommand;

    public DelegateCommand NavigateToPreviousPageCommand
        => _navigateToPreviousPageCommand ??= new DelegateCommand(() => _ = NavigateToPreviousPageCommandExecuteAsync())
            .ObservesCanExecute(() => CanNavigateToPreviousPage);
    protected virtual bool CanNavigateToPreviousPage 
        => CurrentPage > 1 && !IsRefreshing && !IsNavigatingToFirstPage && !IsNavigatingToLastPage && !IsNavigatingToPreviousPage && !IsNavigatingToNextPage;

    private bool _isNavigatingToPreviousPage;
    public bool IsNavigatingToPreviousPage
    {
        get => _isNavigatingToPreviousPage;
        set
        {
            if (SetProperty(ref _isNavigatingToPreviousPage, value))
            {
                RaisePropertyChanged(nameof(CanRefresh));
                RaisePropertyChanged(nameof(CanNavigateToPreviousPage));
                RaisePropertyChanged(nameof(CanNavigateToNextPage));
                RaisePropertyChanged(nameof(CanNavigateToFirstPage));
                RaisePropertyChanged(nameof(CanNavigateToLastPage));
            }
        }
    }

    private async Task NavigateToPreviousPageCommandExecuteAsync()
    {
        try
        {
            IsNavigatingToPreviousPage = true;
            await NavigateToPreviousPageAsync();
        }
        finally
        {
            IsNavigatingToPreviousPage = false;
        }
    }

    protected virtual async Task NavigateToPreviousPageAsync()
    {
        CurrentPage--;
        await RefreshAsync();
    }

    private DelegateCommand? _navigateToNextPageCommand;

    public DelegateCommand NavigateToNextPageCommand
        => _navigateToNextPageCommand ??= new DelegateCommand(() => _ = NavigateToNextPageCommandExecuteAsync())
            .ObservesCanExecute(() => CanNavigateToNextPage);

    protected virtual bool CanNavigateToNextPage 
        => CurrentPage < TotalPage && !IsRefreshing && !IsNavigatingToFirstPage && !IsNavigatingToLastPage && !IsNavigatingToPreviousPage && !IsNavigatingToNextPage;

    private bool _isNavigatingToNextPage;
    public bool IsNavigatingToNextPage
    {
        get => _isNavigatingToNextPage;
        set
        {
            if (SetProperty(ref _isNavigatingToNextPage, value))
            {
                RaisePropertyChanged(nameof(CanRefresh));
                RaisePropertyChanged(nameof(CanNavigateToPreviousPage));
                RaisePropertyChanged(nameof(CanNavigateToNextPage));
                RaisePropertyChanged(nameof(CanNavigateToFirstPage));
                RaisePropertyChanged(nameof(CanNavigateToLastPage));
            }
        }
    }

    private async Task NavigateToNextPageCommandExecuteAsync()
    {
        try
        {
            IsNavigatingToNextPage = true;
            await NavigateToNextPageAsync();
        }
        finally
        {
            IsNavigatingToNextPage = false;
        }
    }

    private async Task NavigateToNextPageAsync()
    {
        CurrentPage++;
        await RefreshAsync();
    }

    private DelegateCommand? _navigateToFirstPageCommand;

    public DelegateCommand NavigateToFirstPageCommand
        => _navigateToFirstPageCommand ??= new DelegateCommand(() => _ = NavigateToFirstPageCommandExecuteAsync())
            .ObservesCanExecute(() => CanNavigateToFirstPage);

    protected virtual bool CanNavigateToFirstPage => CurrentPage > 1 && !IsRefreshing && !IsNavigatingToFirstPage && !IsNavigatingToLastPage && !IsNavigatingToPreviousPage && !IsNavigatingToNextPage;

    private bool _isNavigatingToFirstPage;
    public bool IsNavigatingToFirstPage
    {
        get => _isNavigatingToFirstPage;
        set
        {
            if (SetProperty(ref _isNavigatingToFirstPage, value))
            {
                RaisePropertyChanged(nameof(CanRefresh));
                RaisePropertyChanged(nameof(CanNavigateToPreviousPage));
                RaisePropertyChanged(nameof(CanNavigateToNextPage));
                RaisePropertyChanged(nameof(CanNavigateToFirstPage));
                RaisePropertyChanged(nameof(CanNavigateToLastPage));
            }
        }
    }

    private async Task NavigateToFirstPageCommandExecuteAsync()
    {
        try
        {
            IsNavigatingToFirstPage = true;
            await NavigateToFirstPageAsync();
        }
        finally
        {
            IsNavigatingToFirstPage = false;
        }
    }

    protected virtual async Task NavigateToFirstPageAsync()
    {
        CurrentPage = 1;
        await RefreshAsync();
    }

    private DelegateCommand? _navigateToLastPageCommand;

    public DelegateCommand NavigateToLastPageCommand
        => _navigateToLastPageCommand ??= new DelegateCommand(() => _ = NavigateToLastPageCommandExecuteAsync())
            .ObservesCanExecute(() => CanNavigateToLastPage);

    protected virtual bool CanNavigateToLastPage => CurrentPage < TotalPage;

    private bool _isNavigatingToLastPage;
    public bool IsNavigatingToLastPage
    {
        get => _isNavigatingToLastPage;
        set
        {
            if (SetProperty(ref _isNavigatingToLastPage, value))
            {
                RaisePropertyChanged(nameof(CanRefresh));
                RaisePropertyChanged(nameof(CanNavigateToPreviousPage));
                RaisePropertyChanged(nameof(CanNavigateToNextPage));
                RaisePropertyChanged(nameof(CanNavigateToFirstPage));
                RaisePropertyChanged(nameof(CanNavigateToLastPage));
            }
        }
    }

    private async Task NavigateToLastPageCommandExecuteAsync()
    {
        try
        {
            IsNavigatingToLastPage = true;
            await NavigateToLastPageAsync();
        }
        finally
        {
            IsNavigatingToLastPage = false;
        }
    }

    protected virtual async Task NavigateToLastPageAsync()
    {
        CurrentPage = TotalPage;
        await RefreshAsync();
    }

    private long _totalCount;
    public virtual long TotalCount
    {
        get => _totalCount;
        set
        {
            if (SetProperty(ref _totalCount, value))
            {
                if (CurrentPage > TotalPage)
                {
                    CurrentPage = TotalPage;
                }

                RaisePropertyChanged(nameof(TotalPage));
                RaisePropertyChanged(nameof(CanNavigateToPreviousPage));
                RaisePropertyChanged(nameof(CanNavigateToNextPage));
                RaisePropertyChanged(nameof(CanNavigateToFirstPage));
                RaisePropertyChanged(nameof(CanNavigateToLastPage));
            }
        }
    }

    private int _currentPage = 1;
    public virtual int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                RaisePropertyChanged(nameof(CanNavigateToPreviousPage));
                RaisePropertyChanged(nameof(CanNavigateToNextPage));
                RaisePropertyChanged(nameof(CanNavigateToFirstPage));
                RaisePropertyChanged(nameof(CanNavigateToLastPage));
            }
        }
    }

    public virtual int TotalPage => (int)Math.Ceiling(TotalCount / (double)PageSize);

    private int _pageSize = pageSize;
    public virtual int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                RaisePropertyChanged(nameof(TotalPage));
            }
        }
    }
}
