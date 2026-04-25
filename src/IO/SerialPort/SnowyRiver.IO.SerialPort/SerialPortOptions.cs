using System.IO.Ports;
using SnowyRiver.Configuration;

namespace SnowyRiver.IO.SerialPort;

public class SerialPortOptions : JsonConfiguration
{
    public bool IsMock
    {
        get;
        set => Set(ref field, value);
    }

    public string PortName
    {
        get;
        set => Set(ref field, value);
    } = string.Empty;

    public int BaudRate
    {
        get;
        set => Set(ref field, value);
    } = 115200;

    public Parity Parity
    {
        get;
        set => Set(ref field, value);
    } = Parity.None;

    public int DataBits
    {
        get;
        set => Set(ref field, value);
    } = 8;

    public StopBits StopBits
    {
        get;
        set => Set(ref field, value);
    } = StopBits.One;

    public int ReadTimeout
    {
        get;
        set => Set(ref field, value);
    } = 1000;

    public int WriteTimeout
    {
        get;
        set => Set(ref field, value);
    } = 1000;

    public string NewLine
    {
        get;
        set => Set(ref field, value);
    } = "\r\n";
}
