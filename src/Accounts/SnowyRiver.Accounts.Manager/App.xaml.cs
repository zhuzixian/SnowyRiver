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

            base.OnInitialized();
        }

        private static async Task MigrateAsync()
        {
            var dbContext = ContainerLocator.Container.Resolve<AccountsManagerDbContext>();
            await DbMigrator.MigrateAsync(dbContext);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddDbContext<AccountsManagerDbContext>(options => options.AddAccountsManagerOptions());
            services.AddScoped<DbContext, AccountsManagerDbContext>();
            services.AddUnitOfWork();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<AccountManagerModule>();
        }
    }
}
