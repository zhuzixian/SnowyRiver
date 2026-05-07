using FluentModbus;
using SnowyRiver.Configuration;

namespace SnowyRiver.Modbus.FluentModbus;
public class ModbusTcpClientOptions:JsonConfiguration, ITimeoutProvider
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

    public TimeSpan Timeout
    {
        get;
        set => Set(ref field, value);
    } = System.Threading.Timeout.InfiniteTimeSpan;
}
