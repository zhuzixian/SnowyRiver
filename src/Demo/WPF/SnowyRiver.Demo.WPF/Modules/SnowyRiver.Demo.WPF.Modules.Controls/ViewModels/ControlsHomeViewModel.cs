using System.Drawing;
using System.Windows.Media;
using AIStudio.Wpf.DiagramDesigner;
using Prism.Mvvm;
using Prism.Regions;
using SnowyRiver.Demo.WPF.Core.Mvvm;
using SnowyRiver.WPF.Controls.Diagram;
using SnowyRiver.WPF.Controls.Valves;
using Size = System.Windows.Size;

namespace SnowyRiver.Demo.WPF.Modules.Controls.ViewModels;
public class ControlsHomeViewModel : RegionViewModelBase
{
    public ControlsHomeViewModel(IRegionManager regionManager):base(regionManager)
    {
        DiagramVm.DiagramOption.LayoutOption.PageSizeType = PageSizeType.Custom;
        DiagramVm.DiagramOption.LayoutOption.PageSize = new Size(double.NaN, double.NaN);
        DiagramVm.ColorViewModel.FillColor.Color = Colors.Orange;

        var threePortsValve = new ThreePortsValve();

        var valve = new ContentDesignerItemViewModel(DiagramVm)
        {
            ContentStyle = new ThreePortsValve(),
            Left = 300, Top = 200,
        };
        DiagramVm.Add(valve);


    }

    private DiagramViewModel _diagramVm;
    public DiagramViewModel DiagramVm
        => _diagramVm ??= new DiagramViewModel
        {
            ColorViewModel = new ColorViewModel(),
        };
}
