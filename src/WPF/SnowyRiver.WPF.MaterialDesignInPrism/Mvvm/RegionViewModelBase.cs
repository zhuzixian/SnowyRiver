namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class RegionViewModelBase(IRegionManager regionManager)
    : ViewModelBase, IConfirmNavigationRequest
{
    private IRegionManager _regionManager = regionManager;
    public IRegionManager RegionManager
    {
        get => _regionManager; 
        protected set => SetProperty(ref _regionManager, value);
    }

    public virtual void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
    {
        continuationCallback(true);
    }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public virtual void OnNavigatedFrom(NavigationContext navigationContext)
    {

    }

    public virtual void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.TryGetValue<IRegionManager>(nameof(RegionManager), out var regionManager))
        {
            RegionManager = regionManager;
        }
    }
}
