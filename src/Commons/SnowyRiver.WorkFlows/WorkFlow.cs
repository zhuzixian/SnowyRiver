using SnowyRiver.WPF.NotifyPropertyChangedBase;
using System.Collections.ObjectModel;

namespace SnowyRiver.WorkFlows;
public class WorkFlow<TStep, TKey, TStepKey>: NotifyPropertyChangedObject
     where TStep : WorkStep<TStepKey>
{
    private TKey _id;
    public TKey Id
    {
        get => _id;
        set => Set(ref _id, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private ObservableCollection<TStep> _steps = [];

    public ObservableCollection<TStep> Steps
    {
        get => _steps;
        set => Set(ref _steps, value);
    }

    private DateTime? _startTime;
    public DateTime? StartTime
    {
        get => _startTime;
        set => Set(ref _startTime, value);
    }

    private DateTime? _endTime;
    public DateTime? EndTime
    {
        get => _endTime;
        set => Set(ref _endTime, value);
    }

    private WorkState _state;
    public WorkState State
    {
        get => _state;
        set => Set(ref _state, value);
    }
}
