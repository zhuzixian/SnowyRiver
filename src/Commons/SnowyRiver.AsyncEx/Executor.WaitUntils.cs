using System;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyRiver.AsyncEx;

public static partial class Executor
{
    public static async Task WaitForConditionAsync(
        Func<bool> predicate,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (predicate()) return;

        using var timeoutCts = timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (timeout.HasValue)
        {
            timeoutCts!.CancelAfter(timeout.Value);
        }

        var effectiveToken = timeoutCts?.Token ?? cancellationToken;

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));

        try
        {
            while (await timer.WaitForNextTickAsync(effectiveToken).ConfigureAwait(false))
            {
                if (predicate()) return;
            }
        }
        catch (OperationCanceledException) when (timeout.HasValue && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"The condition was not met within the specified timeout of {timeout.Value}.");
        }
    }

    public static async Task WaitForConditionAsync(
        Func<Task<bool>> predicate,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (await predicate()) return;

        using var timeoutCts = timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (timeout.HasValue)
        {
            timeoutCts!.CancelAfter(timeout.Value);
        }

        var effectiveToken = timeoutCts?.Token ?? cancellationToken;

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));

        try
        {
            while (await timer.WaitForNextTickAsync(effectiveToken).ConfigureAwait(false))
            {
                if (await predicate()) return;
            }
        }
        catch (OperationCanceledException) when (timeout.HasValue && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"The condition was not met within the specified timeout of {timeout.Value}.");
        }
    }
}