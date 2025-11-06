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
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;
using Prism.Dialogs;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SnowyRiver.Accounts.Manager.ViewModels;
using SnowyRiver.Accounts.Services;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.WPF.Modules.Splash;
using SnowyRiver.Reflection;
using SnowyRiver.WPF.MaterialDesignInPrism;
using SnowyRiver.WPF.Modules.Splash.Views;
using Splat.Builder;

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

        protected override void OnExit(ExitEventArgs e)
        {
            CacheDatabase.Shutdown().Wait();
            base.OnExit(e);
        }

        protected override async void OnInitialized()
        {
            try
            {
                AppBuilder.CreateSplatBuilder()
                    .WithAkavacheCacheDatabase<SystemJsonSerializer>(builder =>
                        builder.WithApplicationName(AppDomain.CurrentDomain.FriendlyName)
                            .WithSqliteDefaults());
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
            catch (Exception e)
            {
                //
            }
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

            services.AddAutoMapper(config =>
            {
                config.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            });

            services.AddDbContext<AccountsManagerDbContext>(options => options.AddAccountsManagerOptions());
            services.AddScoped<DbContext, AccountsManagerDbContext>();
            services.AddUnitOfWork();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var productInfo = ReflectionHelper.GetProductInfo();
            containerRegistry.RegisterInstance(productInfo);

            containerRegistry.RegisterSingleton<IAuthenticationService, AuthenticationService>();

            containerRegistry.RegisterSnowyRiverSplashView();
            containerRegistry.RegisterForNavigation<DbMigratorView, DbMigratorViewModel>(WPF.Modules.Splash.ViewNames.DbMigratorView);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<SnowyRiverMaterialDesignModule>();
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<AccountManagerModule>();
            moduleCatalog.AddModule<SnowyRiverSplashModule>();
        }
    }
}
