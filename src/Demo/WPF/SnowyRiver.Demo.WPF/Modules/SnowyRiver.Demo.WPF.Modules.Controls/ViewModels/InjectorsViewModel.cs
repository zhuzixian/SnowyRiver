using Prism.Regions;
using SnowyRiver.Demo.WPF.Core.Mvvm;

namespace SnowyRiver.Demo.WPF.Modules.Controls.ViewModels;
public class InjectorsViewModel : RegionViewModelBase
{
    public InjectorsViewModel(IRegionManager regionManager) :
        base(regionManager)
    {
    }

}
