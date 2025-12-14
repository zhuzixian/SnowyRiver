using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;
public class HasSortIdNotifyPropertyChangedObject : NotifyPropertyChangedObject, IHasSortId
{
    public int SortId
    {
        get;
        set => Set(ref field, value);
    }
}
