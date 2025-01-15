using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using SnowyRiver.WPF.MaterialDesignInPrism.Service;
using SnowyRiver.WPF.MaterialDesignInPrism.ViewModels;
using SnowyRiver.WPF.MaterialDesignInPrism.Views;
using SnowyRiver.WPF.MaterialDesignInPrism.Windows;

namespace SnowyRiver.WPF.MaterialDesignInPrism;
public class SnowyRiverMaterialDesignModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IDialogHostService, DialogHostService>();
        containerRegistry.RegisterDialogWindow<MaterialDesignMetroDialogWindow>();
        containerRegistry.RegisterDialog<DialogView, DialogViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
