namespace SnowyRiver.Notices;

public interface INotifier
{
    bool IsOpen(object identifier);
    void Close(object? dialogIdentifier = null, object? parameter = null);
    Task<string?> ShowAsync(string title, string message, string[] options);
    Task<string?> ShowAsync(string title, string message, string[] options, object identifier);
    Task<(string Title, string Message, string[] Options)?> GetAsync();
    Task<(string Title, string Message, string[] Options)?> GetAsync(object identifier);

    bool IsOpen()
    {
        return IsOpen(DefaultIdentifier);
    }


    public const string DefaultIdentifier = "Root";
}
