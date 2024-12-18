using FluentModbus;
using Polly.Retry;

namespace SnowyRiver.Modbus.FluentModbus;

public class RetryModbusClient(ModbusClient client, AsyncRetryPolicy retryPolicy)
{
    public ModbusClient BaseClient => client;

    public Task<Memory<T>> ReadHoldingRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return retryPolicy.ExecuteAsync(() =>
            client.ReadHoldingRegistersAsync<T>(unitIdentifier, startingAddress, count, cancellationToken));
    }

    public Task<Memory<byte>> ReadHoldingRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default)
    {
        return retryPolicy.ExecuteAsync(() =>
            client.ReadHoldingRegistersAsync(unitIdentifier, startingAddress, quantity, cancellationToken));
    }

    public Task WriteMultipleRegistersAsync<T>(int unitIdentifier, int startingAddress, T[] dataset,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return retryPolicy.ExecuteAsync(() =>
            client.WriteMultipleRegistersAsync(unitIdentifier, startingAddress, dataset, cancellationToken));
    }

    public Task WriteMultipleRegistersAsync(byte unitIdentifier, ushort startingAddress, byte[] dataset,
        CancellationToken cancellationToken = default)
    {
        return retryPolicy.ExecuteAsync(() =>
            client.WriteMultipleRegistersAsync(unitIdentifier, startingAddress, dataset, cancellationToken));
    }

    public Task<Memory<byte>> ReadCoilsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default)
    {
        return retryPolicy.ExecuteAsync(() =>
            client.ReadCoilsAsync(unitIdentifier, startingAddress, quantity, cancellationToken));
    }

    public Task<Memory<byte>> ReadDiscreteInputsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default)
    {
        return retryPolicy.ExecuteAsync(() =>
            client.ReadDiscreteInputsAsync(unitIdentifier, startingAddress, quantity, cancellationToken));
    }

    public async Task<Memory<T>> ReadInputRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged
    {
        return await retryPolicy.ExecuteAsync(() =>
            client.ReadInputRegistersAsync<T>(unitIdentifier, startingAddress, count, cancellationToken));
    }

    public async Task<Memory<byte>> ReadInputRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default)
    {
        return await retryPolicy.ExecuteAsync(() =>
            client.ReadInputRegistersAsync(unitIdentifier, startingAddress, quantity, cancellationToken));
    }

    public async Task WriteSingleCoilAsync(int unitIdentifier, int registerAddress, bool value,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(() =>
            client.WriteSingleCoilAsync(unitIdentifier, registerAddress, value, cancellationToken));
    }

    public async Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, short value,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken));
    }

    public async Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, ushort value,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken));
    }

    public async Task WriteSingleRegisterAsync(byte unitIdentifier, ushort registerAddress, byte[] value,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(() =>
            client.WriteSingleRegisterAsync(unitIdentifier, registerAddress, value, cancellationToken));
    }

    public async Task WriteMultipleCoilsAsync(int unitIdentifier, int startingAddress, bool[] values,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(() => Task.Run(() => 
            client.WriteMultipleCoilsAsync(unitIdentifier, startingAddress, values, cancellationToken), cancellationToken));
    }

    public async Task<Memory<TRead>> ReadWriteMultipleRegistersAsync<TRead, TWrite>(int unitIdentifier,
        int readStartingAddress, int readCount, int writeStartingAddress, TWrite[] dataset,
        CancellationToken cancellationToken = default) where TRead : unmanaged
        where TWrite : unmanaged
    {
        return await retryPolicy.ExecuteAsync(() =>
            client.ReadWriteMultipleRegistersAsync<TRead, TWrite>(unitIdentifier, readStartingAddress, readCount,
                writeStartingAddress, dataset, cancellationToken));
    }

    public async Task<Memory<byte>> ReadWriteMultipleRegistersAsync(byte unitIdentifier, ushort readStartingAddress,
        ushort readQuantity, ushort writeStartingAddress, byte[] dataset, CancellationToken cancellationToken = default)
    {
        return await retryPolicy.ExecuteAsync(() =>
            client.ReadWriteMultipleRegistersAsync(unitIdentifier, readStartingAddress, readQuantity, writeStartingAddress,
                dataset, cancellationToken));
    }
}
