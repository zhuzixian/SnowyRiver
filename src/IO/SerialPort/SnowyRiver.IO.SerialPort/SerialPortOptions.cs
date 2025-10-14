using System.IO.Ports;
using SnowyRiver.Configuration;

namespace SnowyRiver.IO.SerialPort;

public class SerialPortOptions : JsonConfiguration
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

    private Parity _parity = Parity.None;
    public Parity Parity
    {
        get => _parity;
        set => Set(ref _parity, value);
    }

    private int _dataBits = 8;
    public int DataBits
    {
        get => _dataBits;
        set => Set(ref _dataBits, value);
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

    private string _newLine = "\r\n";
    public string NewLine
    {
        get => _newLine;
        set => Set(ref _newLine, value);
    }
}
