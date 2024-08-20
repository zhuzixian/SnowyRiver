using Prism.Navigation.Regions;
using SnowyRiver.Demo.WPF.Core.Mvvm;

namespace SnowyRiver.Demo.WPF.Modules.Controls.ViewModels;
public class InjectorsViewModel(IRegionManager regionManager) : RegionViewModelBase(regionManager);
