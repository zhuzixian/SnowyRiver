namespace SnowyRiver.Commons.Abstractions;

public interface IValueProvider<out T>
{
    public T? Value { get; }
}
