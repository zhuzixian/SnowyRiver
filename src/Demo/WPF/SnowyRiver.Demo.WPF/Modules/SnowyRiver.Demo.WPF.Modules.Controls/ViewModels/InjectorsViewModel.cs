using Prism.Regions;
using SnowyRiver.Demo.WPF.Core.Mvvm;
using SnowyRiver.Demo.WPF.Services.Interfaces;

namespace SnowyRiver.Demo.WPF.Modules.Controls.ViewModels;
public class InjectorsViewModel : RegionViewModelBase
{
    public InjectorsViewModel(IRegionManager regionManager, IMessageService messageService) :
        base(regionManager)
    {
    }

}
