using FluentModbus;
using SnowyRiver.IO.SerialPort;

namespace SnowyRiver.Modbus.FluentModbus;
public class ModbusRtuClientOptions : SerialPortOptions
{
    public ModbusEndianness Endian
    {
        get;
        set => Set(ref field, value);
    } = ModbusEndianness.LittleEndian;
}
