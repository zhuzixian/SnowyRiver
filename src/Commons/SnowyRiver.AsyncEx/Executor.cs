using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace SnowyRiver.AsyncEx;
public static class Executor
{
    public static async Task ExecuteAsync(AsyncLock locker, Func<Task> action, CancellationToken cancellationToken = default)
    {
        using (await locker.LockAsync())
        {
            await action();
        }
    }

    public static async Task<TResult> ExecuteAsync<TResult>(AsyncLock locker, Func<Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        using (await locker.LockAsync())
        {
            return await task.Invoke();
        }
    }

    public static void Execute(AsyncLock locker, Action action)
    {
        using (locker.Lock())
        {
            action.Invoke();
        }
    }

    public static TResult Execute<TResult>(SemaphoreSlim locker, Func<TResult> func)
    {
        using (locker.Lock())
        {
            return func.Invoke();
        }
    }
}
