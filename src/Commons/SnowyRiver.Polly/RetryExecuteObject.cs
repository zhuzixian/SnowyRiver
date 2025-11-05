using Nito.AsyncEx;
using Polly.Retry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyRiver.Polly;
public class RetryExecuteObject(AsyncRetryPolicy retryPolicy)
{
    private static readonly AsyncLock AsyncLocker = new();

    protected async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await Executor.ExecuteAsync(retryPolicy, AsyncLocker, action, cancellationToken);
    }

    protected async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> task,
        CancellationToken cancellationToken = default)
    {
        return await Executor.ExecuteAsync(retryPolicy, AsyncLocker, task, cancellationToken);
    }
}
