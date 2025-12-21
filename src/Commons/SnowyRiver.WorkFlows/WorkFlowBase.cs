using SnowyRiver.ComponentModel.NotifyPropertyChanged.FluentValidation;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SnowyRiver.WorkFlows;
public class WorkFlow<TKey, TState, TStepKey, TStep, TStepState>: ValidatableNotifyPropertyChangedObject
     where TStep : WorkStep<TStepKey, TStepState>
{
    public TKey Id
    {
        get;
        set => Set(ref field, value);
    }

    public string Name
    {
        get;
        set => Set(ref field, value);
    } = string.Empty;

    public ObservableCollection<TStep> Steps
    {
        get;
        set => Set(ref field, value);
    } = [];

    public bool Enable
    {
        get;
        set => Set(ref field, value);
    } = true;

    [JsonIgnore]
    public DateTime? StartTime
    {
        get;
        set => Set(ref field, value);
    }

    [JsonIgnore]
    public DateTime? EndTime
    {
        get;
        set => Set(ref field, value);
    }

    [JsonIgnore]
    public TState? State
    {
        get;
        set => Set(ref field, value);
    }
}
