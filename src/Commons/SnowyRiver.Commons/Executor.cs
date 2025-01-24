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

    public static void Execute(SemaphoreSlim locker, Action action)
    {
        locker.Wait();
        try
        {
            action.Invoke();
        }
        finally
        {
            locker.Release();
        }
    }

    public static TResult Execute<TResult>(SemaphoreSlim locker, Func<TResult> func)
    {
        locker.Wait();
        try
        {
            return func.Invoke();
        }
        finally
        {
            locker.Release();
        }
    }
}
