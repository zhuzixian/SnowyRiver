using System.Threading.Tasks;
using System.Threading;
using System;

namespace SnowyRiver.Commons;
public static class Executor
{
    public static async Task ExecuteAsync(SemaphoreSlim locker, Func<Task> action, CancellationToken cancellationToken = default)
    {
        await locker.WaitAsync(cancellationToken);
        try
        {
            await action();
        }
        finally
        {
            locker.Release();
        }
    }

    public static async Task<TResult> ExecuteAsync<TResult>(SemaphoreSlim locker, Func<Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        await locker.WaitAsync(cancellationToken);
        try
        {
            return await task.Invoke();
        }
        finally
        {
            locker.Release();
        }
    }
}
