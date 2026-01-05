using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.ComponentModel.NotifyPropertyChanged;
public class QueryFilter : NotifyPropertyChangedObject, IQueryFilter
{
    public DateTime? StartTime
    {
        get;
        set => SetProperty(ref field, value);
    } = DateTime.Today;

    public DateTime? EndTime
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int PageIndex
    {
        get;
        set => SetProperty(ref field, value);
    } = 1;

    public int PageSize
    {
        get;
        set => SetProperty(ref field, value);
    } = 16;
}
