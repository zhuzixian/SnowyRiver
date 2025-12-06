using Polly.Retry;
using System;
using System.Threading.Tasks;
using System.Threading;
using Nito.AsyncEx;

namespace SnowyRiver.Polly;
public static class Executor
{
    public static async Task ExecuteAsync(
        AsyncRetryPolicy retryPolicy,
        AsyncLock locker,
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(
            ct => AsyncEx.Executor.ExecuteAsync(locker, action, ct), cancellationToken);
    }

    public static Task<TResult> ExecuteAsync<TResult>(
        AsyncRetryPolicy retryPolicy,
        AsyncLock locker,
        Func<Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        return retryPolicy.ExecuteAsync(
            ct => AsyncEx.Executor.ExecuteAsync(locker, task, ct), cancellationToken);
    }
}
