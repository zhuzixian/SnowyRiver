using System.Windows;
using AIStudio.Wpf.DiagramDesigner;
using AIStudio.Wpf.DiagramDesigner.Models;

namespace SnowyRiver.WPF.Controls.Diagram;
public class ContentDesignerItemViewModel : DesignerItemViewModelBase
{
    public ContentDesignerItemViewModel() : this(null)
    {

    }

    public ContentDesignerItemViewModel(IDiagramViewModel root) : base(root)
    {

    }

    public ContentDesignerItemViewModel(IDiagramViewModel root, SelectableItemBase designer) : base(root, designer)
    {

    }

    public ContentDesignerItemViewModel(IDiagramViewModel root, SerializableItem serializableItem, string serializableType) : base(root, serializableItem, serializableType)
    {
    }

    private Style _contentStyle;
    public Style ContentStyle
    {
        get => _contentStyle;
        set => SetProperty(ref _contentStyle, value);
    }
}
