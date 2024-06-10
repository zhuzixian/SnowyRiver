using Prism.Ioc;
using Prism.Modularity;
using SnowyRiver.Demo.WPF.Services;
using SnowyRiver.Demo.WPF.Services.Interfaces;
using SnowyRiver.Demo.WPF.Views;
using System.Windows;
using SnowyRiver.Demo.WPF.Modules.Controls;

namespace SnowyRiver.Demo.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ControlsNameModule>();
        }
    }
}
