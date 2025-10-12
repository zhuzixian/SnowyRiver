using SnowyRiver.ComponentModel.Interface;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged.Commons;
public class HasSortIdNotifyPropertyChangedObject : NotifyPropertyChangedObject, IHasSortId
{
    private int _sortId;
    public int SortId
    {
        get => _sortId; 
        set => Set(ref _sortId, value);
    }
}
