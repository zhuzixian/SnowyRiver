using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyRiver.Commons.Extensions;

public static class StreamExtensions
{
    public static async Task<byte[]> GetBytesAsync(this Stream stream,
        CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }
}
