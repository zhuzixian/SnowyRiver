using System.IO.Ports;
using FluentModbus;
using SnowyRiver.WPF.Configuration;

namespace SnowyRiver.Modbus.FluentModbus;
public class ModbusOptions : JsonConfiguration
{
    public bool IsMock { get; set; }

    private string _portName = string.Empty;

    public string PortName
    {
        get => _portName;
        set => Set(ref _portName, value);
    }

    private int _baudRate = 115200;

    public int BaudRate
    {
        get => _baudRate; 
        set => Set(ref _baudRate, value);
    }

    private Parity _parity = Parity.Even;
    public Parity Parity
    {
        get => _parity; 
        set => Set(ref _parity, value);
    }

    private StopBits _stopBits = StopBits.One;
    public StopBits StopBits
    {
        get => _stopBits; 
        set => Set(ref _stopBits, value);
    }

    private int _readTimeout = 1000;

    public int ReadTimeout
    {
        get => _readTimeout;
        set => Set(ref _readTimeout, value);
    }

    private int _writeTimeout = 1000;
    public int WriteTimeout
    {
        get => _writeTimeout;
        set => Set(ref _writeTimeout, value);
    }

    private ModbusEndianness _endian = ModbusEndianness.LittleEndian;
    public ModbusEndianness Endian
    {
        get => _endian;
        set => Set(ref _endian, value);
    }
}
