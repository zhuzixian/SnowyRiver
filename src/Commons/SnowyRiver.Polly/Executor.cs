using Polly.Retry;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace SnowyRiver.Polly;
public static class Executor
{
    public static async Task ExecuteAsync(
        AsyncRetryPolicy retryPolicy,
        SemaphoreSlim locker,
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(
            () => Commons.Executor.ExecuteAsync(locker, action, cancellationToken));
    }

    public static Task<TResult> ExecuteAsync<TResult>(
        AsyncRetryPolicy retryPolicy,
        SemaphoreSlim locker,
        Func<Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        return retryPolicy.ExecuteAsync(
            () => Commons.Executor.ExecuteAsync(locker, task, cancellationToken));
    }
}
