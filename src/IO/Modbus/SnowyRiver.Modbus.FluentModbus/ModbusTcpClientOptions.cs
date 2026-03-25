using FluentModbus;
using SnowyRiver.Configuration;

namespace SnowyRiver.Modbus.FluentModbus;
public class ModbusTcpClientOptions:JsonConfiguration
{
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
