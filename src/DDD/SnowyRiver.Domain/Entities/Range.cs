namespace SnowyRiver.Domain.Entities;

public class DoubleRange : Range<double>
{
    public DoubleRange()
    {
    }
    public DoubleRange(double? min, double? max) : base(min, max)
    {
    }
}

public class Range: Range<int>
{
    public Range()
    {
    }
    public Range(int? min, int? max) : base(min, max)
    {
    }
}

public class Range<T> where T : struct
{
    public Range()
    {
    }

    public Range(T? min, T? max)
    {
        Min = min;
        Max = max;
    }

    public T? Min { get; set; }
    public T? Max { get; set; }
}
