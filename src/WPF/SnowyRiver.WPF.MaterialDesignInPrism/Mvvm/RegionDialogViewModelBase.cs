﻿namespace SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
public class RegionDialogViewModelBase(IRegionManager regionManager) : DialogViewModel, IConfirmNavigationRequest
{
    protected IRegionManager RegionManager { get; private set; } = regionManager;

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

    }
}
