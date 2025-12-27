namespace SnowyRiver.Notices;

public interface INotifier
{
    bool IsOpen(object dialogIdentifier);
    void Close(object? dialogIdentifier = null, object? parameter = null);
    Task<string?> ShowAsync(string title, string message, string[] options);
    Task<string?> ShowAsync(string title, string message, string[] options, object identifier);

    bool IsOpen()
    {
        return IsOpen(DefaultIdentifier);
    }

    public const string DefaultIdentifier = "Root";
}
