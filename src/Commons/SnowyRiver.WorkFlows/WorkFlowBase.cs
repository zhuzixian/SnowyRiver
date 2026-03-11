using SnowyRiver.ComponentModel.NotifyPropertyChanged.FluentValidation;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using SnowyRiver.ComponentModel.NotifyPropertyChanged;

namespace SnowyRiver.WorkFlows;
public class WorkFlow<TKey, TState, TStepKey, TStep, TStepState, T>
    : ValidatableNotifyPropertyChangedObject<T>
    where T: WorkFlow<TKey, TState, TStepKey, TStep, TStepState, T>
     where TStep : WorkStep<TStepKey, TStepState, TStep>

{
    public TKey Id
    {
        get;
        set => Set(ref field, value);
    }

    [TrackHistory]
    public string Name
    {
        get;
        set => Set(ref field, value);
    } = string.Empty;

    [TrackHistory]
    public ObservableCollection<TStep> Steps
    {
        get;
        set => Set(ref field, value);
    } = [];

    [TrackHistory]
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
