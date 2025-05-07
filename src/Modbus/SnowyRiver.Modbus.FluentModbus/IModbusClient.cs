namespace SnowyRiver.Modbus.FluentModbus;
public interface IModbusClient
{
    public bool IsConnected { get; }
    public Task<T[]> ReadHoldingRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged;

    public Task<byte[]> ReadHoldingRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default);

    public Task WriteMultipleRegistersAsync<T>(int unitIdentifier, int startingAddress, T[] dataset,
        CancellationToken cancellationToken = default) where T : unmanaged;

    public Task WriteMultipleRegistersAsync(byte unitIdentifier, ushort startingAddress, byte[] dataset,
        CancellationToken cancellationToken = default);

    public Task<Memory<byte>> ReadCoilsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default);

    public Task<byte[]> ReadDiscreteInputsAsync(int unitIdentifier, int startingAddress, int quantity,
        CancellationToken cancellationToken = default);

    public Task<T[]> ReadInputRegistersAsync<T>(int unitIdentifier, int startingAddress, int count,
        CancellationToken cancellationToken = default) where T : unmanaged;

    public Task<byte[]> ReadInputRegistersAsync(byte unitIdentifier, ushort startingAddress,
        ushort quantity, CancellationToken cancellationToken = default);

    public Task WriteSingleCoilAsync(int unitIdentifier, int registerAddress, bool value,
        CancellationToken cancellationToken = default);

    public Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, short value,
        CancellationToken cancellationToken = default);

    public Task WriteSingleRegisterAsync(int unitIdentifier, int registerAddress, ushort value,
        CancellationToken cancellationToken = default);

    public Task WriteSingleRegisterAsync(byte unitIdentifier, ushort registerAddress, byte[] value,
        CancellationToken cancellationToken = default);

    public Task WriteMultipleCoilsAsync(int unitIdentifier, int startingAddress, bool[] values,
        CancellationToken cancellationToken = default);

    public Task<TRead[]> ReadWriteMultipleRegistersAsync<TRead, TWrite>(int unitIdentifier,
        int readStartingAddress, int readCount, int writeStartingAddress, TWrite[] dataset,
        CancellationToken cancellationToken = default) where TRead : unmanaged
        where TWrite : unmanaged;

    public Task<byte[]> ReadWriteMultipleRegistersAsync(byte unitIdentifier, ushort readStartingAddress,
        ushort readQuantity, ushort writeStartingAddress, byte[] dataset,
        CancellationToken cancellationToken = default);
}
