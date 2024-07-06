using System.Windows;
using System.Windows.Controls;
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

    private Control _content;
    public Control Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}
