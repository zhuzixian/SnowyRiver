using System;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyRiver.AsyncEx;

public static partial class Executor
{
    public static async Task WaitForConditionAsync(
        Func<bool> predicate,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));

        while (!predicate())
        {
            try
            {
                await timer.WaitForNextTickAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }
    }
}