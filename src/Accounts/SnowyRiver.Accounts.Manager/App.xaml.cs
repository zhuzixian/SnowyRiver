using Prism.Ioc;
using SnowyRiver.Accounts.Manager.Views;
using System.Globalization;
using System.Windows;
using Prism.Modularity;
using SnowyRiver.Accounts.Modules.Manager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkCore.UnitOfWork.Extensions;
using SnowyRiver.Accounts.Manager.EntityFramework;
using System;
using System.Threading.Tasks;
using Prism.Dialogs;
using SnowyRiver.Accounts.Modules.Manager.Services;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;
using SnowyRiver.WPF.MaterialDesignInPrism.Windows;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SnowyRiver.Accounts.Manager.ViewModels;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Services;
using SnowyRiver.WPF.Modules.Splash;
using SnowyRiver.Commons;
using SnowyRiver.WPF.Modules.Splash.Views;

namespace SnowyRiver.Accounts.Manager
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

        protected override async void OnInitialized()
        {
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = CultureInfo.CurrentCulture;

            try
            {
                await MigrateAsync();
            }
            catch (Exception e)
            {
                var errorMessage = $"The database error: {(e.InnerException != null ? e.InnerException.Message : e.Message)}";
                if (MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK) == MessageBoxResult.OK)
                {
                    Environment.Exit(0);
                }
            }

            var dialogService = ContainerLocator.Container.Resolve<IDialogService>();
            dialogService.ShowDialog(WPF.Modules.Splash.ViewNames.SplashView);

            base.OnInitialized();
        }

        private static async Task MigrateAsync()
        {
            var dbContext = ContainerLocator.Container.Resolve<AccountsManagerDbContext>();
            await DbMigrator.MigrateAsync(dbContext);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(config =>
            {
                config.ClearProviders();
                config.SetMinimumLevel(LogLevel.Trace);
                config.AddNLog();
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddDbContext<AccountsManagerDbContext>(options => options.AddAccountsManagerOptions());
            services.AddScoped<DbContext, AccountsManagerDbContext>();
            services.AddUnitOfWork();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var productInfo = ReflectionHelper.GetProductInfo();
            containerRegistry.RegisterInstance(productInfo);

            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.RegisterDialogWindow<MaterialDesignMetroDialogWindow>();
            containerRegistry.RegisterSingleton<IAuthenticationService<User, Team, Role, Permission>, AuthenticationService>();

            containerRegistry.RegisterSnowyRiverSplashView();
            containerRegistry.RegisterForNavigation<DbMigratorView, DbMigratorViewModel>(WPF.Modules.Splash.ViewNames.DbMigratorView);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<AccountManagerModule>();
            moduleCatalog.AddModule<SnowyRiverSplashModule>();
        }
    }
}
