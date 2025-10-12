using System.Text.Json;
using SnowyRiver.Serializer.Interfaces;

namespace SnowyRiver.Serializer;
public class JsonSerializer(JsonSerializerOptions options) : IJsonSerializer
{
    public async Task<string> SerializeAsync<T>(T data, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await System.Text.Json.JsonSerializer.SerializeAsync(stream, data, options, cancellationToken);
        stream.Position = 0;
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    public async Task<T?> DeserializeAsync<T>(string? data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return default;
        }

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data));
        return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
    }
}
