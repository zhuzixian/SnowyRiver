using FluentModbus;
using SnowyRiver.IO.SerialPort;

namespace SnowyRiver.Modbus.FluentModbus;
public class ModbusRtuClientOptions : SerialPortOptions, ITimeoutProvider
{
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
