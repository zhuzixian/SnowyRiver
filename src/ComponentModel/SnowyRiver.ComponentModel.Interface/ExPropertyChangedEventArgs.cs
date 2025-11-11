using System.ComponentModel;

namespace SnowyRiver.ComponentModel.Interface;

public class ExPropertyChangedEventArgs(string? propertyName,
    object? oldValue, object? newValue) : PropertyChangedEventArgs(propertyName)
{
    public object? OldValue { get; } = oldValue;
    public object? NewValue { get; } = newValue;
}
