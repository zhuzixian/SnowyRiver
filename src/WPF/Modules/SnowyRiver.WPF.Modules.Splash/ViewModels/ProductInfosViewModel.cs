using System;
using System.IO;
using System.Windows.Media.Imaging;
using Prism.Navigation.Regions;
using SnowyRiver.Products;
using SnowyRiver.Reflection;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.WPF.Modules.Splash.ViewModels;
public class ProductInfosViewModel: RegionViewModelBase
{
    public ProductInfosViewModel(ProductInfo productInfo, IRegionManager regionManager):base(regionManager)
    {
        ProductInfo = productInfo;

        var logoFileInfo = new FileInfo("./Resources/logo.png");
        if (logoFileInfo.Exists)
        {
            Logo = new BitmapImage(new Uri(logoFileInfo.FullName));
        }
    }

    private ProductInfo _productInfo;
    public ProductInfo ProductInfo
    {
        get => _productInfo;
        set => SetProperty(ref _productInfo, value);
    }

    private BitmapImage _logo;
    public BitmapImage Logo
    {
        get => _logo;
        set => SetProperty(ref _logo, value);
    }
}
