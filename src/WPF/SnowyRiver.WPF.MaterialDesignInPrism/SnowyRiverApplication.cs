using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;
using System.Diagnostics;
using System.Windows;
using Container = DryIoc.Container;

namespace SnowyRiver.WPF.MaterialDesignInPrism;
public abstract class SnowyRiverApplication : PrismApplication
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!IsSupportMultiProcess)
        {
            var thisIsOnlyProcess = GetCurrentIsOnlyProcess();
            if (!thisIsOnlyProcess)
            {
                NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME,
                    IntPtr.Zero, IntPtr.Zero);
                Environment.Exit(-1);
                return;
            }
        }

        base.OnStartup(e);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        MainWindow?.Activate();
    }

    protected virtual bool GetCurrentIsOnlyProcess()
    {
        var thisProc = Process.GetCurrentProcess();
        return Process.GetProcessesByName(thisProc.ProcessName).Length <= 1;
    }

    protected override IContainerExtension CreateContainerExtension()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        return new DryIocContainerExtension(new Container(CreateContainerRules())
            .WithDependencyInjectionAdapter(serviceCollection));
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected override void RegisterRequiredTypes(IContainerRegistry containerRegistry)
    {
        base.RegisterRequiredTypes(containerRegistry);
        containerRegistry.Register<IDialogHostService, DialogHostService>();
    }

    protected virtual bool IsSupportMultiProcess => true;
}
