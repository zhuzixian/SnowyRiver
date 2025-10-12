using System.Text.Json.Serialization;
using SnowyRiver.ComponentModel.NotifyPropertyChanged;

namespace SnowyRiver.WorkFlows;
public class WorkStep<TKey, TSate>: NotifyPropertyChangedObject
{
    private TKey _id;
    public TKey Id
    {
        get => _id;
        set => Set(ref _id, value);
    }

    private int _sortId;
    public int SortId
    {
        get => _sortId;
        set => Set(ref _sortId, value);
    }

    private bool _enable;
    public bool Enable
    {
        get => _enable;
        set => Set(ref _enable, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private TSate? _state;
    [JsonIgnore]
    public TSate? State
    {
        get => _state;
        set => Set(ref _state, value);
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

    private int _cycleCount = 1;
    /// <summary>
    /// 执行次数
    /// </summary>
    public int CycleCount
    {
        get => _cycleCount;
        set => Set(ref _cycleCount, value);
    }

    private int _cycleIndex;
    /// <summary>
    /// 完成次数
    /// </summary>
    [JsonIgnore]
    public int CycleIndex
    {
        get => _cycleIndex;
        set => Set(ref _cycleIndex, value);
    }
}
