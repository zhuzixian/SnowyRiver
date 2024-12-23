using FluentModbus;
using Polly.Retry;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusClient(ModbusClient client, AsyncRetryPolicy retryPolicy)
{
    public ModbusClient BaseClient => client;

    protected SemaphoreSlim AsyncLocker = new(1);

    public Task ExecuteAsync(Func<Task> action)
    {
        return retryPolicy.ExecuteAsync(async () =>
        {
            await AsyncLocker.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                AsyncLocker.Release();
            }
        });
    }

    protected Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
    {
        return retryPolicy.ExecuteAsync(async () =>
        {
            await AsyncLocker.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                AsyncLocker.Release();
            }
        });
    }

    public Task<Memory<T>> ReadHoldingRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync(() =>
            client.ReadHoldingRegistersAsync<T>(unitIdentifier, startingAddress, count, cancellationToken));
    }

    public Task<Memory<byte>> ReadHoldingRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.ReadHoldingRegistersAsync(unitIdentifier, startingAddress, quantity, cancellationToken));
    }

    public Task WriteMultipleRegistersAsync<T>(int unitIdentifier, int startingAddress, T[] dataset,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync(() =>
            client.WriteMultipleRegistersAsync(unitIdentifier, startingAddress, dataset, cancellationToken));
    }

    public Task WriteMultipleRegistersAsync(byte unitIdentifier, ushort startingAddress, byte[] dataset,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteMultipleRegistersAsync(unitIdentifier, startingAddress, dataset, cancellationToken));
    }

    public Task<Memory<byte>> ReadCoilsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.ReadCoilsAsync(unitIdentifier, startingAddress, quantity, cancellationToken));
    }

    public Task<Memory<byte>> ReadDiscreteInputsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.ReadDiscreteInputsAsync(unitIdentifier, startingAddress, quantity, cancellationToken));
    }

    public Task<Memory<T>> ReadInputRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return ExecuteAsync(() =>
            client.ReadInputRegistersAsync<T>(unitIdentifier, startingAddress, count, cancellationToken));
    }

    public Task<Memory<byte>> ReadInputRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.ReadInputRegistersAsync(unitIdentifier, startingAddress, quantity, cancellationToken));
    }

    public Task WriteSingleCoilAsync(int unitIdentifier, int registerAddress, bool value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteSingleCoilAsync(unitIdentifier, registerAddress, value, cancellationToken));
    }

    public Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, short value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken));
    }

    public Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, ushort value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken));
    }

    public Task WriteSingleRegisterAsync(byte unitIdentifier, ushort registerAddress, byte[] value,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken));
    }

    public Task WriteMultipleCoilsAsync(int unitIdentifier, int startingAddress, bool[] values,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() => Task.Run(() => 
            client.WriteMultipleCoilsAsync(unitIdentifier, startingAddress, values, cancellationToken), cancellationToken));
    }

    public Task<Memory<TRead>> ReadWriteMultipleRegistersAsync<TRead, TWrite>(int unitIdentifier,
        int readStartingAddress, int readCount, int writeStartingAddress, TWrite[] dataset,
        CancellationToken cancellationToken = default) where TRead : unmanaged
        where TWrite : unmanaged
    {
        return ExecuteAsync(() =>
            client.ReadWriteMultipleRegistersAsync<TRead, TWrite>(unitIdentifier, readStartingAddress, readCount,
                writeStartingAddress, dataset, cancellationToken));
    }

    public Task<Memory<byte>> ReadWriteMultipleRegistersAsync(byte unitIdentifier, ushort readStartingAddress,
        ushort readQuantity, ushort writeStartingAddress, byte[] dataset, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() =>
            client.ReadWriteMultipleRegistersAsync(unitIdentifier, readStartingAddress, readQuantity, writeStartingAddress,
                dataset, cancellationToken));
    }
}
