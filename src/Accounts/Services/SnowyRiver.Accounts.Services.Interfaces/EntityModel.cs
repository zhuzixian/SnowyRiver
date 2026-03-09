using SnowyRiver.ComponentModel.NotifyPropertyChanged;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Services.Interfaces;
public class EntityModel : NotifyPropertyChangedObject,IHasSortId
{
    public Guid Id { get; set; }

    public DateTime CreationTime
    {
        get;
        set => Set(ref field, value);
    } = DateTime.Now;

    public int SortId
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public bool IsSelected
    {
        get;
        set => SetProperty(ref field, value);
    }
}
