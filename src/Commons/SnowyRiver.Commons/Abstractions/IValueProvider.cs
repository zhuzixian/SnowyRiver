namespace SnowyRiver.Commons.Abstractions;

public interface IValueProvider<T>
{
    public T? Value { get; set; }
}
