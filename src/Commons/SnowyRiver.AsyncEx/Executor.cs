using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace SnowyRiver.AsyncEx;
public static class Executor
{
    public static async Task ExecuteAsync(AsyncLock locker, Func<Task> action, CancellationToken cancellationToken = default)
    {
        using var lockerDisposable = await locker.LockAsync(cancellationToken);
        await action();
    }

    public static async Task<TResult> ExecuteAsync<TResult>(AsyncLock locker, Func<Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        using var lockerDisposable = await locker.LockAsync(cancellationToken);
        return await task.Invoke();
    }

    public static void Execute(AsyncLock locker, Action action)
    {
        using var lockerDisposable = locker.Lock();
        action.Invoke();
    }

    public static TResult Execute<TResult>(SemaphoreSlim locker, Func<TResult> func)
    {
        using var lockerDisposable = locker.Lock();
        return func.Invoke();
    }
}
