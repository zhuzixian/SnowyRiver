using FluentModbus;
using Nito.AsyncEx;
using Polly.Retry;
using SnowyRiver.Polly;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusClient(ModbusClient client, AsyncRetryPolicy retryPolicy):IModbusClient
{
    protected DateTime LastAccessTime = DateTime.MinValue;

    public TimeSpan MinAccessIntervalTime { get; set; } = TimeSpan.MinValue;

    protected AsyncLock AsyncLocker = new();

    public Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        return Executor.ExecuteAsync(retryPolicy, AsyncLocker, async () =>
            {
                await WaitingMinAccessIntervalTimeAsync(cancellationToken);
                await action();
                LastAccessTime = DateTime.Now;
            }, cancellationToken);
    }

    protected Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        return Executor.ExecuteAsync(retryPolicy, AsyncLocker,
            async () =>
            {
                await WaitingMinAccessIntervalTimeAsync(cancellationToken);
                var result = await task.Invoke();
                LastAccessTime = DateTime.Now;
                return result;
            }, cancellationToken);
    }

    protected async Task WaitingMinAccessIntervalTimeAsync(CancellationToken cancellationToken)
    {
        while (MinAccessIntervalTime > TimeSpan.Zero && DateTime.Now - LastAccessTime < MinAccessIntervalTime)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
        }
    }

    public bool IsConnected => client.IsConnected;

    public virtual Task<T[]> ReadHoldingRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync(async () =>
            {
                var memoryData = await client.ReadHoldingRegistersAsync<T>(unitIdentifier, startingAddress, count, cancellationToken);
                return memoryData.Span.ToArray();
            },
            cancellationToken);
    }

    public virtual Task<byte[]> ReadHoldingRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async() =>
        {
            var memoryData = await client.ReadHoldingRegistersAsync(unitIdentifier, startingAddress, quantity, cancellationToken);
            return memoryData.Span.ToArray();
        },
        cancellationToken);
    }

    public virtual Task WriteMultipleRegistersAsync<T>(int unitIdentifier, int startingAddress, T[] dataset,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync(async () =>
        {
            // TODO: Fix this issue
            // Corruption of array passed to WriteMultipleRegistersAsync
            // https://github.com/Apollo3zehn/FluentModbus/issues/52
            var sendDataSet = (T[])dataset.Clone();
            await client.WriteMultipleRegistersAsync(unitIdentifier, startingAddress, sendDataSet, cancellationToken);
        }, cancellationToken);
    }

    public virtual Task WriteMultipleRegistersAsync(byte unitIdentifier, ushort startingAddress, byte[] dataset,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
                client.WriteMultipleRegistersAsync(unitIdentifier, startingAddress, dataset, cancellationToken),
            cancellationToken);
    }

    public virtual Task<Memory<byte>> ReadCoilsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.ReadCoilsAsync(unitIdentifier, startingAddress, quantity, cancellationToken), cancellationToken);
    }

    public virtual Task<byte[]> ReadDiscreteInputsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () =>
            {
                var memoryData = await client.ReadDiscreteInputsAsync(unitIdentifier, startingAddress, quantity, cancellationToken);
                return memoryData.Span.ToArray();
            }, cancellationToken);
    }

    public virtual Task<T[]> ReadInputRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync( async () =>
            {
               var memoryData = await client.ReadInputRegistersAsync<T>(unitIdentifier, startingAddress, count, cancellationToken);
               return memoryData.Span.ToArray();
            },cancellationToken);
    }

    public virtual Task<byte[]> ReadInputRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () =>
        {
            var memoryData = await client.ReadInputRegistersAsync(unitIdentifier, startingAddress, quantity, cancellationToken);
            return memoryData.Span.ToArray();
        }, cancellationToken);
    }

    public virtual Task WriteSingleCoilAsync(int unitIdentifier, int registerAddress, bool value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteSingleCoilAsync(unitIdentifier, registerAddress, value, cancellationToken), cancellationToken);
    }

    public virtual Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, short value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken), cancellationToken);
    }

    public virtual Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, ushort value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken), cancellationToken);
    }

    public virtual Task WriteSingleRegisterAsync(byte unitIdentifier, ushort registerAddress, byte[] value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken), cancellationToken);
    }

    public virtual Task WriteMultipleCoilsAsync(int unitIdentifier, int startingAddress, bool[] values,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteMultipleCoilsAsync(unitIdentifier, startingAddress, values, cancellationToken), cancellationToken);
    }

    public virtual Task<TRead[]> ReadWriteMultipleRegistersAsync<TRead, TWrite>(int unitIdentifier,
        int readStartingAddress, int readCount, int writeStartingAddress, TWrite[] dataset,
        CancellationToken cancellationToken = default) where TRead : unmanaged
        where TWrite : unmanaged
    {
        return ExecuteAsync(async () =>
        {
            var memoryData = await client.ReadWriteMultipleRegistersAsync<TRead, TWrite>(unitIdentifier,
                readStartingAddress, readCount,
                writeStartingAddress, dataset, cancellationToken);
            return memoryData.Span.ToArray();
        }, cancellationToken);
    }

    public virtual Task<byte[]> ReadWriteMultipleRegistersAsync(byte unitIdentifier, ushort readStartingAddress,
        ushort readQuantity, ushort writeStartingAddress, byte[] dataset, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async () =>
        {
            var memoryData = await client.ReadWriteMultipleRegistersAsync(unitIdentifier, readStartingAddress,
                readQuantity,
                writeStartingAddress,
                dataset, cancellationToken);
            return memoryData.Span.ToArray();
        }, cancellationToken);
    }
}
