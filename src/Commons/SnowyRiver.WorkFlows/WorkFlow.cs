using SnowyRiver.WPF.NotifyPropertyChangedBase;
using System.Collections.ObjectModel;

namespace SnowyRiver.WorkFlows;
public class WorkFlow<TStep>: NotifyPropertyChangedObject
     where TStep : WorkStep
{
    private string _name = string.Empty;
    public virtual string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private  ObservableCollection<TStep> _steps = [];

    public virtual ObservableCollection<TStep> Steps
    {
        get => _steps;
        set => Set(ref _steps, value);
    }

    private DateTime? _startTime;
    public virtual DateTime? StartTime
    {
        get => _startTime;
        set => Set(ref _startTime, value);
    }

    private DateTime? _endTime;
    public virtual DateTime? EndTime
    {
        get => _endTime;
        set => Set(ref _endTime, value);
    }
}
