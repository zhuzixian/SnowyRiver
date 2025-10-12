using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using SnowyRiver.ComponentModel.NotifyPropertyChanged;

namespace SnowyRiver.WorkFlows;
public class WorkFlow<TKey, TState, TStepKey, TStep, TStepState>: NotifyPropertyChangedObject
     where TStep : WorkStep<TStepKey, TStepState>
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

    private bool _enable = true;
    public bool Enable
    {
        get => _enable;
        set => Set(ref _enable, value);
    }

    private DateTime? _startTime;
    [JsonIgnore]
    public DateTime? StartTime
    {
        get => _startTime;
        set => Set(ref _startTime, value);
    }

    private DateTime? _endTime;
    [JsonIgnore]
    public DateTime? EndTime
    {
        get => _endTime;
        set => Set(ref _endTime, value);
    }

    private TState? _state;
    [JsonIgnore]
    public TState? State
    {
        get => _state;
        set => Set(ref _state, value);
    }
}
