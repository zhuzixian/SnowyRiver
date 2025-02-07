using System.Text.Json.Serialization;
using SnowyRiver.WPF.NotifyPropertyChangedBase;

namespace SnowyRiver.WorkFlows;
public class WorkStep<TSate>: NotifyPropertyChangedObject
{
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

    private TSate _state;
    [JsonIgnore]
    public TSate State
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
}
