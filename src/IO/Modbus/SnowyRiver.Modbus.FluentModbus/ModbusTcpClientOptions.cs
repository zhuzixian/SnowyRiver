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
}
