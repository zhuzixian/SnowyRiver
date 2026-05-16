using FluentModbus;
using Nito.AsyncEx;
using Polly.Retry;
using SnowyRiver.Polly;

namespace SnowyRiver.Modbus.FluentModbus;

public abstract class RetryModbusClient(
    ITimeoutProvider timeoutProvider,
    ModbusClient client, 
    AsyncRetryPolicy retryPolicy):IModbusClient
{
    public DateTime LastAccessTime { get; protected set; } = DateTime.MinValue;

    public TimeSpan MinAccessIntervalTime { get; set; } = TimeSpan.MinValue;

    protected readonly AsyncLock AsyncLocker = new();

    public Task ExecuteAsync(Func<CancellationToken, Task> action, 
        bool isUpdateLastAccessTime = false,
        CancellationToken cancellationToken = default)
    {
        return ExecuteCoreAsync<object?>(async ct =>
        {
            await action(ct).ConfigureAwait(false);
            return null;
        }, isUpdateLastAccessTime, cancellationToken);
    }

    protected Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> task,
        bool isUpdateLastAccessTime = true,
        CancellationToken cancellationToken = default)
    {
        return ExecuteCoreAsync(task, isUpdateLastAccessTime, cancellationToken);
    }

    private Task<TResult> ExecuteCoreAsync<TResult>(Func<CancellationToken, Task<TResult>> task,
        bool isUpdateLastAccessTime,
        CancellationToken cancellationToken)
    {
        return Executor.ExecuteAsync(retryPolicy, AsyncLocker, async () =>
        {
            await WaitingMinAccessIntervalTimeAsync(cancellationToken).ConfigureAwait(false);

            var hasTimeout = timeoutProvider.Timeout > TimeSpan.Zero;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (hasTimeout)
            {
                cts.CancelAfter(timeoutProvider.Timeout);
            }

            try
            {
                return await task(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (hasTimeout && !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"The Modbus operation did not complete within the specified timeout of {timeoutProvider.Timeout}.");
            }
            finally
            {
                if (isUpdateLastAccessTime)
                {
                    LastAccessTime = DateTime.Now;
                }
            }
        }, cancellationToken);
    }


    protected async Task WaitingMinAccessIntervalTimeAsync(CancellationToken cancellationToken)
    {
        while (MinAccessIntervalTime > TimeSpan.Zero && DateTime.Now - LastAccessTime < MinAccessIntervalTime)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
        }
    }

    public abstract void Connect();
    public abstract void Close();

    public bool IsConnected => client.IsConnected;

    public virtual Task<T[]> ReadHoldingRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync(async ct =>
            {
                var memoryData = await client.ReadHoldingRegistersAsync<T>(unitIdentifier, startingAddress, count, ct);
                return memoryData.Span.ToArray();
            },
            cancellationToken:cancellationToken);
    }

    public virtual Task<byte[]> ReadHoldingRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async ct =>
        {
            var memoryData = await client.ReadHoldingRegistersAsync(unitIdentifier, startingAddress, quantity, ct);
            return memoryData.Span.ToArray();
        },
        cancellationToken:cancellationToken);
    }

    public virtual Task WriteMultipleRegistersAsync<T>(int unitIdentifier, int startingAddress, T[] dataset,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync(async ct =>
        {
            // TODO: Fix this issue
            // Corruption of array passed to WriteMultipleRegistersAsync
            // https://github.com/Apollo3zehn/FluentModbus/issues/52
            var sendDataSet = (T[])dataset.Clone();
            await client.WriteMultipleRegistersAsync(unitIdentifier, startingAddress, sendDataSet, ct);
        }, cancellationToken:cancellationToken);
    }

    public virtual Task WriteMultipleRegistersAsync(byte unitIdentifier, ushort startingAddress, byte[] dataset,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(ct =>
                client.WriteMultipleRegistersAsync(unitIdentifier, startingAddress, dataset, ct),
            cancellationToken: cancellationToken);
    }

    public virtual Task<Memory<byte>> ReadCoilsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(ct =>
            client.ReadCoilsAsync(unitIdentifier, startingAddress, quantity, 
                ct), cancellationToken:cancellationToken);
    }

    public virtual Task<byte[]> ReadDiscreteInputsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async ct =>
            {
                var memoryData = await client.ReadDiscreteInputsAsync(unitIdentifier, startingAddress, quantity, ct);
                return memoryData.Span.ToArray();
            }, cancellationToken:cancellationToken);
    }

    public virtual Task<T[]> ReadInputRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync( async ct =>
            {
               var memoryData = await client.ReadInputRegistersAsync<T>(unitIdentifier, startingAddress, count, ct);
               return memoryData.Span.ToArray();
            }, cancellationToken:cancellationToken);
    }

    public virtual Task<byte[]> ReadInputRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async ct =>
        {
            var memoryData = await client.ReadInputRegistersAsync(unitIdentifier, startingAddress, quantity, ct);
            return memoryData.Span.ToArray();
        }, cancellationToken:cancellationToken);
    }

    public virtual Task WriteSingleCoilAsync(int unitIdentifier, int registerAddress, bool value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(ct =>
            client.WriteSingleCoilAsync(unitIdentifier, registerAddress, value, ct), 
            cancellationToken: cancellationToken);
    }

    public virtual Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, short value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(ct =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, ct), 
            cancellationToken: cancellationToken);
    }

    public virtual Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, ushort value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(ct =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, ct), 
            cancellationToken: cancellationToken);
    }

    public virtual Task WriteSingleRegisterAsync(byte unitIdentifier, ushort registerAddress, byte[] value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(ct =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, ct),
            cancellationToken: cancellationToken);
    }

    public virtual Task WriteMultipleCoilsAsync(int unitIdentifier, int startingAddress, bool[] values,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(ct =>
            client.WriteMultipleCoilsAsync(unitIdentifier, startingAddress, values, ct), 
            cancellationToken: cancellationToken);
    }

    public virtual Task<TRead[]> ReadWriteMultipleRegistersAsync<TRead, TWrite>(int unitIdentifier,
        int readStartingAddress, int readCount, int writeStartingAddress, TWrite[] dataset,
        CancellationToken cancellationToken = default) where TRead : unmanaged
        where TWrite : unmanaged
    {
        return ExecuteAsync(async ct =>
        {
            var memoryData = await client.ReadWriteMultipleRegistersAsync<TRead, TWrite>(unitIdentifier,
                readStartingAddress, readCount,
                writeStartingAddress, dataset, ct);
            return memoryData.Span.ToArray();
        },  cancellationToken: cancellationToken);
    }

    public virtual Task<byte[]> ReadWriteMultipleRegistersAsync(byte unitIdentifier, ushort readStartingAddress,
        ushort readQuantity, ushort writeStartingAddress, byte[] dataset, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async ct =>
        {
            var memoryData = await client.ReadWriteMultipleRegistersAsync(unitIdentifier, readStartingAddress,
                readQuantity,
                writeStartingAddress,
                dataset, ct);
            return memoryData.Span.ToArray();
        }, cancellationToken:cancellationToken);
    }

    public async Task<bool[]> ReadCoilsAsBoolArrayAsync(int unitIdentifier, int startingAddress, int quantity, CancellationToken cancellationToken)
    {
        var values = new bool[quantity];
        var bytes = await ReadCoilsAsync(unitIdentifier, startingAddress, quantity, cancellationToken);
        for (var i = 0; i < quantity; i++)
        {
            var byteIndex = i / 8;
            var bitIndex = i % 8;

            if (byteIndex < bytes.Length)
            {
                values[i] = (bytes.Span[byteIndex] & (1 << bitIndex)) != 0;
            }
        }
        return values;
    }
}
