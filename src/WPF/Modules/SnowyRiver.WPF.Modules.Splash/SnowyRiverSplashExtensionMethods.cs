using Prism.Ioc;
using SnowyRiver.WPF.Modules.Splash.Views;

namespace SnowyRiver.WPF.Modules.Splash;
public static class SnowyRiverSplashExtensionMethods
{
    public static IContainerRegistry RegisterSnowyRiverSplashView(this IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<ProductInfosView>(ViewNames.ProductInfoView);
        containerRegistry.RegisterForNavigation<WelcomeView>(ViewNames.WelcomeView);
        containerRegistry.RegisterForNavigation<InitializationView>(ViewNames.InitializationView);
        containerRegistry.RegisterForNavigation<DbMigratorView>(ViewNames.DbMigratorView);
        return containerRegistry;
    }
}
