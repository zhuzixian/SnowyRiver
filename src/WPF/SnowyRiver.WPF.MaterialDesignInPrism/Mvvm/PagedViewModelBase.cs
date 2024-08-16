using Prism.Regions;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class PagedViewModelBase(RegionManager regionManager):RegionViewModelBase(regionManager)
{
    private long _totalCount;
    public long TotalCount
    {
        get => _totalCount;
        set
        {
            if (SetProperty(ref _totalCount, value))
            {
                RaisePropertyChanged(nameof(TotalPage));
            }
        }
    }

    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public int TotalPage => (int)Math.Ceiling(TotalCount / (double)PageSize);

    private int _pageSize = 100;
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
