using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace SnowyRiver.AsyncEx;

public static class Executor
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
}