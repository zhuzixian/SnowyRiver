using Prism.Ioc;
using Prism.Modularity;
using SnowyRiver.WPF.Modules.Splash.Views;

namespace SnowyRiver.WPF.Modules.Splash
{
    public class SnowyRiverSplashModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<SplashView>(ViewNames.SplashView);
        }
    }
}