using SnowyRiver.ComponentModel.NotifyPropertyChanged.FluentValidation;
using System.Text.Json.Serialization;

namespace SnowyRiver.WorkFlows;
public class WorkStep<TKey, TSate>: ValidatableNotifyPropertyChangedObject
{
    public TKey Id
    {
        get;
        set => Set(ref field, value);
    }

    public int SortId
    {
        get;
        set => Set(ref field, value);
    }

    public bool Enable
    {
        get;
        set => Set(ref field, value);
    }

    public string Name
    {
        get;
        set => Set(ref field, value);
    } = string.Empty;

    [JsonIgnore]
    public TSate? State
    {
        get;
        set => Set(ref field, value);
    }

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

    /// <summary>
    /// 执行次数
    /// </summary>
    public int CycleCount
    {
        get;
        set => Set(ref field, value);
    } = 1;

    /// <summary>
    /// 完成次数
    /// </summary>
    [JsonIgnore]
    public int CycleIndex
    {
        get;
        set => Set(ref field, value);
    }
}
