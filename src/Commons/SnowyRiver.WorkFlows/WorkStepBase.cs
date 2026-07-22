using SnowyRiver.ComponentModel.NotifyPropertyChanged.FluentValidation;
using System.Text.Json.Serialization;
using SnowyRiver.ComponentModel.NotifyPropertyChanged;

namespace SnowyRiver.WorkFlows;
public class WorkStep<TKey, TSate>: ValidatableNotifyPropertyChangedObject<WorkStep<TKey, TSate>>
{
    public TKey Id
    {
        get;
        set => SetProperty(ref field, value);
    }

    [TrackHistory]
    public int SortId
    {
        get;
        set => SetProperty(ref field, value);
    }

    [TrackHistory]
    public bool Enable
    {
        get;
        set => SetProperty(ref field, value);
    }

    [TrackHistory]
    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    [JsonIgnore]
    public TSate? State
    {
        get;
        set => SetProperty(ref field, value);
    }

    [JsonIgnore]
    public DateTime? StartTime
    {
        get;
        set => SetProperty(ref field, value);
    }

    [JsonIgnore]
    public DateTime? EndTime
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// 执行次数
    /// </summary>
    [TrackHistory]
    public int CycleCount
    {
        get;
        set => SetProperty(ref field, value);
    } = 1;

    /// <summary>
    /// 完成次数
    /// </summary>
    [JsonIgnore]
    public int CycleIndex
    {
        get;
        set => SetProperty(ref field, value);
    }
}
