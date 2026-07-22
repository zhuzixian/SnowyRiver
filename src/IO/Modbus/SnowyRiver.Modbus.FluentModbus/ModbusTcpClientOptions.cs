using FluentModbus;
using SnowyRiver.Configuration;

namespace SnowyRiver.Modbus.FluentModbus;
public class ModbusTcpClientOptions:JsonConfiguration, ITimeoutProvider
{
    public bool IsMock
    {
        get; 
        set => SetProperty(ref field, value);
    }


    public string? RemoteEndpoint
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ModbusEndianness Endian
    {
        get;
        set => SetProperty(ref field, value);
    } = ModbusEndianness.LittleEndian;

    public TimeSpan Timeout
    {
        get;
        set => SetProperty(ref field, value);
    } = System.Threading.Timeout.InfiniteTimeSpan;
}
