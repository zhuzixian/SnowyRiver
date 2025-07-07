using FluentModbus;
using SnowyRiver.IO.SerialPort;

namespace SnowyRiver.Modbus.FluentModbus;
public class ModbusRtuClientOptions : SerialPortOptions
{
    private ModbusEndianness _endian = ModbusEndianness.LittleEndian;
    public ModbusEndianness Endian
    {
        get => _endian;
        set => Set(ref _endian, value);
    }
}
