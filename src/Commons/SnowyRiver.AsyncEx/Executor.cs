using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace SnowyRiver.AsyncEx;

public static partial class Executor
{
    public static async Task ExecuteAsync(AsyncLock locker, Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(action);

        using var lockerDisposable = await locker.LockAsync(cancellationToken).ConfigureAwait(false);
        await action().ConfigureAwait(false);
    }

    public static async Task<TResult> ExecuteAsync<TResult>(AsyncLock locker, Func<Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(task);

        using var lockerDisposable = await locker.LockAsync(cancellationToken).ConfigureAwait(false);
        return await task().ConfigureAwait(false);
    }

    public static async ValueTask ExecuteAsync(AsyncLock locker, Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(action);

        using var lockerDisposable = await locker.LockAsync(cancellationToken).ConfigureAwait(false);
        await action().ConfigureAwait(false);
    }

    public static async ValueTask<TResult> ExecuteAsync<TResult>(AsyncLock locker, Func<ValueTask<TResult>> task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(task);

        using var lockerDisposable = await locker.LockAsync(cancellationToken).ConfigureAwait(false);
        return await task().ConfigureAwait(false);
    }

    public static void Execute(AsyncLock locker, Action action)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(action);

        using var lockerDisposable = locker.Lock();
        action();
    }

    public static TResult Execute<TResult>(AsyncLock locker, Func<TResult> func)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(func);

        using var lockerDisposable = locker.Lock();
        return func();
    }

    public static async Task ExecuteAsync(SemaphoreSlim locker, Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(action);

        await locker.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            locker.Release();
        }
    }

    public static TResult Execute<TResult>(SemaphoreSlim locker, Func<TResult> func)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(func);

        using var lockerDisposable = locker.Lock();
        return func();
    }

    public static async Task<TResult> ExecuteAsync<TResult>(SemaphoreSlim locker, Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locker);
        ArgumentNullException.ThrowIfNull(action);

        await locker.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await action().ConfigureAwait(false);
        }
        finally
        {
            locker.Release();
        }
    }
}