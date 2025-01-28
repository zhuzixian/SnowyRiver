using System;

namespace SnowyRiver.WPF.NotifyPropertyChangedBase.Commons;
public class QueryFilter : NotifyPropertyChangedObject
{
    private DateTime? _startTime = DateTime.Today;

    public DateTime? StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    private DateTime? _endTime;
    public DateTime? EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    private int _pageIndex = 1;
    public int PageIndex
    {
        get => _pageIndex;
        set => SetProperty(ref _pageIndex, value);
    }

    private int _pageSize = 16;

    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }
}
