using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using DryIoc.Microsoft.DependencyInjection;
using Container = DryIoc.Container;
using System.Windows;

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

    protected virtual bool IsSupportMultiProcess => true;
}
