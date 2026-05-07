using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace SnowyRiver.Modbus.FluentModbus;

public static class ModbusRetryPolicies
{
    public static AsyncRetryPolicy CreateWithReconnect(
        int retryCount,
        Func<int, TimeSpan> sleepDurationProvider,
        Action reconnect,
        ILogger? logger = null)
    {
        return Policy
            .Handle<TimeoutException>()
            .Or<ObjectDisposedException>()
            .Or<IOException>()
            .Or<System.Net.Sockets.SocketException>()
            .WaitAndRetryAsync(
                retryCount,
                sleepDurationProvider,
                onRetry: (ex, delay, attempt, _) =>
                {
                    logger?.LogWarning(ex,
                        "Modbus operation failed (attempt {Attempt}). Reconnecting before retry...", attempt);
                    try { reconnect(); }
                    catch (Exception rex)
                    {
                        logger?.LogError(rex, "Reconnect before retry failed.");
                    }
                });
    }
}
