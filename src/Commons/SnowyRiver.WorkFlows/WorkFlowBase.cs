using SnowyRiver.WPF.NotifyPropertyChangedBase;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SnowyRiver.WorkFlows;
public class WorkFlow<TState, TStep, TStepState>: NotifyPropertyChangedObject
     where TStep : WorkStep<TStepState>
{
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

    private TState _state;
    [JsonIgnore]
    public TState State
    {
        get => _state;
        set => Set(ref _state, value);
    }
}
