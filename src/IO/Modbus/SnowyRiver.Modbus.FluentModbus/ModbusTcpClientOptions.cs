using FluentModbus;
using SnowyRiver.Configuration;

namespace SnowyRiver.Modbus.FluentModbus;
public class ModbusTcpClientOptions:JsonConfiguration
{
    public bool IsMock
    {
        get; 
        set => Set(ref field, value);
    }


    public string? RemoteEndpoint
    {
        get;
        set => Set(ref field, value);
    }

    public ModbusEndianness Endian
    {
        get;
        set => Set(ref field, value);
    } = ModbusEndianness.LittleEndian;

    public TimeSpan? ReadTimeout
    {
        get;
        set => Set(ref field, value);
    }

    public TimeSpan? WriteTimeout
    {
        get;
        set => Set(ref field, value);
    }
}
