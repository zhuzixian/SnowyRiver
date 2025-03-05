using Prism.Navigation.Regions;
using SnowyRiver.Reflection;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;
public class ProductInfosViewModel(ProductInfo productInfo, IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    private ProductInfo _productInfo = productInfo;
    public ProductInfo ProductInfo
    {
        get => _productInfo;
        set => SetProperty(ref _productInfo, value);
    }
}
