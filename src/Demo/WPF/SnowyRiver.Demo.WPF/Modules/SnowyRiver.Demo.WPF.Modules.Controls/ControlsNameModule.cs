using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using SnowyRiver.Demo.WPF.Core;
using SnowyRiver.Demo.WPF.Modules.Controls.Views;

namespace SnowyRiver.Demo.WPF.Modules.Controls
{
    public class ControlsNameModule(IRegionManager regionManager) : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            regionManager.RequestNavigate(RegionNames.ControlsRegion, ViewNames.ControlsView);
            regionManager.RequestNavigate(RegionNames.ControlsHomeRegion, ViewNames.ControlsHomeView);
            
            regionManager.RequestNavigate(RegionNames.InjectorsRegion, ViewNames.InjectorsView);
            regionManager.RequestNavigate(RegionNames.ValvesRegion, ViewNames.ValvesView);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<InjectorsView>(ViewNames.InjectorsView);
            containerRegistry.RegisterForNavigation<ValvesView>(ViewNames.ValvesView);
            containerRegistry.RegisterForNavigation<ControlsHomeView>(ViewNames.ControlsHomeView);
            containerRegistry.RegisterForNavigation<ControlsView>(ViewNames.ControlsView);
        }
    }
}